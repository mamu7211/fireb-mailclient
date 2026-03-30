#!/usr/bin/env bash
# git-worktree-manager.sh — TUI for managing git worktrees
# Works on Linux and Windows (Git Bash)
set -uo pipefail

# ── Terminal Check ───────────────────────────────────────────────────
if [[ ! -t 0 || ! -t 1 ]]; then
    echo "error: this tool requires an interactive terminal" >&2
    exit 1
fi

# ── Colors & Formatting ─────────────────────────────────────────────
BOLD=$'\033[1m'
DIM=$'\033[2m'
RESET=$'\033[0m'
RED=$'\033[31m'
GREEN=$'\033[32m'
YELLOW=$'\033[33m'
BLUE=$'\033[34m'
CYAN=$'\033[36m'
WHITE=$'\033[37m'
BG_BLUE=$'\033[44m'

# ── Helpers ──────────────────────────────────────────────────────────
die()   { printf "%s\n" "${RED}error:${RESET} $*" >&2; exit 1; }
info()  { printf "%s\n" "${CYAN}>>>${RESET} $*"; }
warn()  { printf "%s\n" "${YELLOW}warning:${RESET} $*"; }

clear_screen() { printf $'\033[2J\033[H'; }

# Draw a horizontal rule
hr() {
    local cols
    cols=$(tput cols 2>/dev/null || echo 80)
    local line=""
    for ((i = 0; i < cols; i++)); do line+="─"; done
    printf "%s\n" "${DIM}${line}${RESET}"
}

# Require git
command -v git >/dev/null 2>&1 || die "git is not installed"

# Must be inside a git repo
git rev-parse --git-dir >/dev/null 2>&1 || die "not inside a git repository"

MAIN_WORKTREE=$(git worktree list --porcelain | head -1 | sed 's/^worktree //')

# ── Data Collection ──────────────────────────────────────────────────

# Collect worktree info into parallel arrays
declare -a WT_PATHS=()
declare -a WT_BRANCHES=()
declare -a WT_COMMITS=()
declare -a WT_DIRTY=()
declare -a WT_AHEAD=()
declare -a WT_BEHIND=()
declare -a WT_UNTRACKED=()
declare -a WT_STAGED=()
declare -a WT_COMMIT_DATE=()
declare -a WT_LAST_MODIFIED=()

refresh_worktrees() {
    WT_PATHS=()
    WT_BRANCHES=()
    WT_COMMITS=()
    WT_DIRTY=()
    WT_AHEAD=()
    WT_BEHIND=()
    WT_UNTRACKED=()
    WT_STAGED=()
    WT_COMMIT_DATE=()
    WT_LAST_MODIFIED=()

    local path="" branch="" commit="" bare=""

    while IFS= read -r line; do
        case "$line" in
            "worktree "*)
                path="${line#worktree }"
                branch=""
                commit=""
                bare=""
                ;;
            "HEAD "*)
                commit="${line#HEAD }"
                commit="${commit:0:8}"
                ;;
            "branch "*)
                branch="${line#branch refs/heads/}"
                ;;
            "detached")
                branch="(detached)"
                ;;
            "bare")
                bare=1
                ;;
            "")
                if [[ -n "$path" && -z "$bare" ]]; then
                    WT_PATHS+=("$path")
                    WT_BRANCHES+=("${branch:-"(unknown)"}")
                    WT_COMMITS+=("$commit")

                    # Get status counts by running git in that worktree
                    local dirty=0 untracked=0 staged=0
                    local status_output
                    status_output=$(git -C "$path" status --porcelain 2>/dev/null || true)

                    if [[ -n "$status_output" ]]; then
                        while IFS= read -r sline; do
                            local xy="${sline:0:2}"
                            case "$xy" in
                                "??"*) untracked=$((untracked + 1)) ;;
                                *)
                                    [[ "${xy:0:1}" != " " && "${xy:0:1}" != "?" ]] && staged=$((staged + 1))
                                    [[ "${xy:1:1}" != " " && "${xy:1:1}" != "?" ]] && dirty=$((dirty + 1))
                                    ;;
                            esac
                        done <<< "$status_output"
                    fi

                    WT_DIRTY+=("$dirty")
                    WT_UNTRACKED+=("$untracked")
                    WT_STAGED+=("$staged")

                    # Ahead/behind upstream
                    local ab
                    ab=$(git -C "$path" rev-list --left-right --count "@{upstream}...HEAD" 2>/dev/null || echo "0 0")
                    local behind_n ahead_n
                    behind_n=$(echo "$ab" | awk '{print $1}')
                    ahead_n=$(echo "$ab" | awk '{print $2}')
                    WT_AHEAD+=("$ahead_n")
                    WT_BEHIND+=("$behind_n")

                    # Latest commit date (short format)
                    local commit_date
                    commit_date=$(git -C "$path" log -1 --format='%cd' --date=short 2>/dev/null || echo "—")
                    WT_COMMIT_DATE+=("$commit_date")

                    # Latest file modification date in worktree (newest tracked or untracked file)
                    local last_mod
                    if find "$path" -maxdepth 0 -printf '' 2>/dev/null; then
                        # GNU find (Linux)
                        last_mod=$(find "$path" -maxdepth 3 -not -path '*/.git/*' -type f -printf '%T@ %TY-%Tm-%Td\n' 2>/dev/null \
                            | sort -rn | head -1 | awk '{print $2}')
                    else
                        # BSD/Git Bash fallback: use stat
                        last_mod=$(find "$path" -maxdepth 3 -not -path '*/.git/*' -type f -exec stat --format='%Y' {} + 2>/dev/null \
                            || find "$path" -maxdepth 3 -not -path '*/.git/*' -type f -exec stat -f '%m' {} + 2>/dev/null)
                        if [[ -n "$last_mod" ]]; then
                            local newest_ts
                            newest_ts=$(echo "$last_mod" | sort -rn | head -1)
                            # Convert epoch to YYYY-MM-DD
                            if date -d "@$newest_ts" '+%Y-%m-%d' >/dev/null 2>&1; then
                                last_mod=$(date -d "@$newest_ts" '+%Y-%m-%d')
                            else
                                last_mod=$(date -r "$newest_ts" '+%Y-%m-%d' 2>/dev/null || echo "—")
                            fi
                        fi
                    fi
                    WT_LAST_MODIFIED+=("${last_mod:-"—"}")
                fi
                path=""
                ;;
        esac
    done < <(git worktree list --porcelain; echo "")
}

# ── Display ──────────────────────────────────────────────────────────

print_header() {
    local cols
    cols=$(tput cols 2>/dev/null || echo 80)
    printf "${BG_BLUE}${WHITE}${BOLD}"
    printf " Git Worktree Manager"
    printf "%*s" $((cols - 21)) ""
    printf "${RESET}\n"
    printf "${DIM}  repo: %s${RESET}\n" "$MAIN_WORKTREE"
    hr
}

print_worktree_list() {
    local count=${#WT_PATHS[@]}

    if [[ $count -eq 0 ]]; then
        printf "%s\n" "  ${DIM}No worktrees found.${RESET}"
        return
    fi

    # Calculate max branch name length for dynamic column width
    local max_branch=6  # minimum: length of "BRANCH"
    for i in $(seq 0 $((count - 1))); do
        local blen=${#WT_BRANCHES[$i]}
        [[ $blen -gt $max_branch ]] && max_branch=$blen
    done

    # Print header
    printf "  ${BOLD}%-4s %-${max_branch}s  %-8s  %-10s  %-10s  %-8s  %s${RESET}\n" \
        "#" "BRANCH" "COMMIT" "COMMITTED" "MODIFIED" "STATUS" "PATH"
    hr

    for i in $(seq 0 $((count - 1))); do
        local path="${WT_PATHS[$i]}"
        local branch="${WT_BRANCHES[$i]}"
        local commit="${WT_COMMITS[$i]}"
        local dirty="${WT_DIRTY[$i]}"
        local untracked="${WT_UNTRACKED[$i]}"
        local staged="${WT_STAGED[$i]}"
        local ahead="${WT_AHEAD[$i]}"
        local behind="${WT_BEHIND[$i]}"

        # Build status string (plain text for column width, colored for display)
        local status_parts=()
        local status_plain_parts=()
        if [[ "$staged" -gt 0 ]]; then
            status_parts+=("${GREEN}S(${staged})${RESET}")
            status_plain_parts+=("S(${staged})")
        fi
        if [[ "$dirty" -gt 0 ]]; then
            status_parts+=("${RED}M(${dirty})${RESET}")
            status_plain_parts+=("M(${dirty})")
        fi
        if [[ "$untracked" -gt 0 ]]; then
            status_parts+=("${YELLOW}U(${untracked})${RESET}")
            status_plain_parts+=("U(${untracked})")
        fi
        if [[ "$ahead" -gt 0 ]]; then
            status_parts+=("${CYAN}A(${ahead})${RESET}")
            status_plain_parts+=("A(${ahead})")
        fi
        if [[ "$behind" -gt 0 ]]; then
            status_parts+=("${RED}B(${behind})${RESET}")
            status_plain_parts+=("B(${behind})")
        fi

        local status_str status_plain
        if [[ ${#status_parts[@]} -eq 0 ]]; then
            status_str="${GREEN}clean${RESET}"
            status_plain="clean"
        else
            status_str=$(IFS=" "; echo "${status_parts[*]}")
            status_plain=$(IFS=" "; echo "${status_plain_parts[*]}")
        fi

        # Pad status manually to account for ANSI codes
        local status_width=8
        local pad=$((status_width - ${#status_plain}))
        [[ $pad -lt 0 ]] && pad=0
        local status_padded="${status_str}$(printf '%*s' "$pad" '')"

        # Mark main worktree
        local marker=""
        if [[ "$path" == "$MAIN_WORKTREE" ]]; then
            marker=" ${DIM}(main)${RESET}"
        fi

        # Shorten path for display
        local display_path="$path"
        if [[ "$display_path" == "$HOME"* ]]; then
            display_path="~${display_path#"$HOME"}"
        fi

        local num=$((i + 1))
        local commit_date="${WT_COMMIT_DATE[$i]}"
        local last_mod="${WT_LAST_MODIFIED[$i]}"
        printf "  ${BOLD}%-4s${RESET} ${BLUE}%-${max_branch}s${RESET}  ${DIM}%-8s${RESET}  %-10s  %-10s  %s  %s%s\n" \
            "$num" "$branch" "$commit" "$commit_date" "$last_mod" "$status_padded" "$display_path" "$marker"
    done

    # Legend
    printf "\n  ${DIM}S=staged  M=modified  U=untracked  A=ahead  B=behind${RESET}\n"
}

print_menu() {
    echo ""
    hr
    printf "  ${BOLD}Actions:${RESET}  "
    printf "${CYAN}[a]${RESET}dd  "
    printf "${CYAN}[r]${RESET}emove  "
    printf "${CYAN}[s]${RESET}tatus  "
    printf "${CYAN}[f]${RESET}etch  "
    printf "${CYAN}[p]${RESET}rune  "
    printf "${CYAN}[c]${RESET}d  "
    printf "${CYAN}[R]${RESET}efresh  "
    printf "${CYAN}[q]${RESET}uit\n"
}

# ── Actions ──────────────────────────────────────────────────────────

select_worktree() {
    local prompt="${1:-"Select worktree #: "}"
    local num
    read -rp "  $prompt" num
    if ! [[ "$num" =~ ^[0-9]+$ ]] || [[ "$num" -lt 1 || "$num" -gt ${#WT_PATHS[@]} ]]; then
        warn "invalid selection"
        return 1
    fi
    echo $((num - 1))
}

# Map branch name to a short worktree path
# feature/#135-toast-showcase  → ../<repo>/135-toast-showcase
# fix/#139-sidebar-wcag        → ../<repo>/139-sidebar-wcag
# fix/156-toolbar-save         → ../<repo>/156-toolbar-save
# some-branch                  → ../<repo>/some-branch
worktree_path_for_branch() {
    local branch="$1" repo_name="$2"
    local parent
    parent="$(dirname "$MAIN_WORKTREE")"

    # Strip type prefix (feature/, fix/, bug/, chore/, docs/, etc.)
    local rest="$branch"
    rest="${rest#feature/}"
    rest="${rest#fix/}"
    rest="${rest#bug/}"
    rest="${rest#chore/}"
    rest="${rest#docs/}"
    rest="${rest#refactor/}"

    # Strip leading #
    rest="${rest#\#}"

    echo "${parent}/${repo_name}/${rest}"
}

action_add() {
    echo ""
    local branch path input

    # Collect available branches sorted by most recent commit, excluding already checked-out
    local -a available_branches=()
    local checked_out
    checked_out=$(git worktree list --porcelain | grep '^branch ' | sed 's|^branch refs/heads/||')

    # Local branches sorted by last commit date (newest first), max 10
    while IFS= read -r b; do
        if ! echo "$checked_out" | grep -qxF "$b"; then
            available_branches+=("$b")
        fi
        [[ ${#available_branches[@]} -ge 10 ]] && break
    done < <(git branch --sort=-committerdate --format='%(refname:short)' 2>/dev/null)

    # Fill remaining slots with remote-only branches (also by date)
    if [[ ${#available_branches[@]} -lt 10 ]]; then
        while IFS= read -r b; do
            b="${b#origin/}"
            [[ "$b" == "HEAD" ]] && continue
            # Skip if already in local list or checked out
            local already=0
            for existing in "${available_branches[@]}"; do
                [[ "$existing" == "$b" ]] && already=1 && break
            done
            echo "$checked_out" | grep -qxF "$b" && already=1
            if [[ $already -eq 0 ]]; then
                available_branches+=("origin/$b")
                [[ ${#available_branches[@]} -ge 10 ]] && break
            fi
        done < <(git branch -r --sort=-committerdate --format='%(refname:short)' 2>/dev/null)
    fi

    # Display numbered list
    if [[ ${#available_branches[@]} -gt 0 ]]; then
        printf "  ${BOLD}Available branches:${RESET}\n"
        for i in $(seq 0 $((${#available_branches[@]} - 1))); do
            printf "  ${CYAN}%3d${RESET}  %s\n" "$((i + 1))" "${available_branches[$i]}"
        done
        echo ""
    fi

    read -rp "  Branch # or new name: " input
    [[ -z "$input" ]] && { warn "aborted"; return; }

    # If input is a number, select from the list
    if [[ "$input" =~ ^[0-9]+$ ]] && [[ "$input" -ge 1 && "$input" -le ${#available_branches[@]} ]]; then
        branch="${available_branches[$((input - 1))]}"
        # Strip origin/ prefix for remote branches
        branch="${branch#origin/}"
        info "selected branch '$branch'"
    else
        branch="$input"
    fi

    local default_path repo_name
    repo_name=$(basename "$MAIN_WORKTREE")
    default_path=$(worktree_path_for_branch "$branch" "$repo_name")
    read -rp "  Path [$default_path]: " path
    path="${path:-$default_path}"

    # Create parent directory if needed
    mkdir -p "$(dirname "$path")"

    if git show-ref --verify --quiet "refs/heads/$branch" 2>/dev/null; then
        info "checking out existing branch '$branch'"
        git worktree add "$path" "$branch"
    elif git show-ref --verify --quiet "refs/remotes/origin/$branch" 2>/dev/null; then
        info "tracking remote branch 'origin/$branch'"
        git worktree add --track -b "$branch" "$path" "origin/$branch"
    else
        info "creating new branch '$branch' from HEAD"
        git worktree add -b "$branch" "$path"
    fi

    info "worktree created at $path"
}

action_remove() {
    echo ""
    local idx
    idx=$(select_worktree "Remove worktree #: ") || return

    local path="${WT_PATHS[$idx]}"
    local branch="${WT_BRANCHES[$idx]}"

    if [[ "$path" == "$MAIN_WORKTREE" ]]; then
        warn "cannot remove the main worktree"
        return
    fi

    # Check for uncommitted changes
    local dirty="${WT_DIRTY[$idx]}"
    local staged="${WT_STAGED[$idx]}"
    if [[ "$dirty" -gt 0 || "$staged" -gt 0 ]]; then
        warn "worktree has uncommitted changes ($dirty modified, $staged staged)"
        read -rp "  Force remove? [y/N]: " confirm
        [[ "$confirm" =~ ^[yY]$ ]] || { info "aborted"; return; }
        git worktree remove --force "$path"
    else
        git worktree remove "$path"
    fi

    # Optionally delete the branch
    read -rp "  Also delete branch '$branch'? [y/N]: " del_branch
    if [[ "$del_branch" =~ ^[yY]$ ]]; then
        git branch -d "$branch" 2>/dev/null || git branch -D "$branch"
        info "branch '$branch' deleted"
    fi

    info "worktree removed"
}

action_status() {
    echo ""
    local idx
    idx=$(select_worktree "Show status for #: ") || return

    local path="${WT_PATHS[$idx]}"
    local branch="${WT_BRANCHES[$idx]}"

    printf "\n  ${BOLD}Worktree: ${BLUE}%s${RESET}\n" "$branch"
    printf "  ${DIM}Path: %s${RESET}\n\n" "$path"

    # Recent commits
    printf "  ${BOLD}Recent commits:${RESET}\n"
    git -C "$path" log --oneline -5 --decorate | while IFS= read -r line; do
        printf "    %s\n" "$line"
    done

    echo ""

    # Status
    printf "  ${BOLD}Working tree status:${RESET}\n"
    local status_output
    status_output=$(git -C "$path" status --short 2>/dev/null)
    if [[ -z "$status_output" ]]; then
        printf "    ${GREEN}clean${RESET}\n"
    else
        echo "$status_output" | while IFS= read -r line; do
            printf "    %s\n" "$line"
        done
    fi

    # Upstream info
    echo ""
    local upstream
    upstream=$(git -C "$path" rev-parse --abbrev-ref "@{upstream}" 2>/dev/null || echo "none")
    printf "  ${BOLD}Upstream:${RESET} %s\n" "$upstream"
}

action_fetch() {
    echo ""
    info "fetching from all remotes..."
    git fetch --all --prune
    info "done"
}

action_prune() {
    echo ""
    info "pruning stale worktree references..."
    git worktree prune -v
    info "done"
}

action_cd() {
    echo ""
    local idx
    idx=$(select_worktree "Open shell in #: ") || return

    local path="${WT_PATHS[$idx]}"
    local branch="${WT_BRANCHES[$idx]}"

    info "opening subshell in $path (exit to return)"
    printf "  ${DIM}branch: %s${RESET}\n" "$branch"
    (cd "$path" && exec "$SHELL")
}

# ── Main Loop ────────────────────────────────────────────────────────

main() {
    while true; do
        clear_screen
        refresh_worktrees
        print_header
        print_worktree_list
        print_menu

        local choice
        read -rsn1 choice
        echo ""

        case "$choice" in
            a|A) action_add ;;
            r)   action_remove ;;
            s)   action_status ;;
            f)   action_fetch ;;
            p)   action_prune ;;
            c)   action_cd ;;
            R)   continue ;;  # refresh
            q|Q) clear_screen; exit 0 ;;
            *)   warn "unknown action '$choice'" ;;
        esac

        echo ""
        read -rsn1 -p "  Press any key to continue..."
    done
}

main "$@"
