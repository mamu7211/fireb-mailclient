/**
 * Accessibility module for SidebarMenu.
 * Syncs aria-current="page" with NavLink's active class so assistive
 * technologies can announce the current page.
 */

/**
 * Sets aria-current="page" on the active NavLink and observes DOM mutations
 * to keep it in sync after client-side navigation.
 * @param {HTMLElement} navElement
 */
export function syncAriaCurrent(navElement) {
    applyAriaCurrent(navElement);

    const observer = new MutationObserver(() => applyAriaCurrent(navElement));
    observer.observe(navElement, { attributes: true, attributeFilter: ['class'], subtree: true });
}

/**
 * @param {HTMLElement} navElement
 */
function applyAriaCurrent(navElement) {
    for (const link of navElement.querySelectorAll('.sidebar-menu-entry')) {
        if (link.classList.contains('active')) {
            link.setAttribute('aria-current', 'page');
        } else {
            link.removeAttribute('aria-current');
        }
    }
}
