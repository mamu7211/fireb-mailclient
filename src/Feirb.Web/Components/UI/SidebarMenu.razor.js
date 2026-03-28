/**
 * Keyboard navigation module for SidebarMenu (WCAG AA).
 * Implements roving tabindex with Arrow Up/Down, Home, End.
 */

/**
 * @param {HTMLElement} navElement
 */
export function initKeyboardNav(navElement) {
    const getItems = () => Array.from(navElement.querySelectorAll('[role="menuitem"]'));

    // Set initial roving tabindex: active item or first item gets tabindex 0.
    const items = getItems();
    const activeItem = items.find(i => i.classList.contains('active'));
    items.forEach(item => item.setAttribute('tabindex', '-1'));
    if (activeItem) {
        activeItem.setAttribute('tabindex', '0');
    } else if (items.length > 0) {
        items[0].setAttribute('tabindex', '0');
    }

    // Sync aria-current="page" with NavLink active class.
    syncAriaCurrent(navElement);

    // Observe class changes to keep aria-current in sync after navigation.
    const observer = new MutationObserver(() => syncAriaCurrent(navElement));
    observer.observe(navElement, { attributes: true, attributeFilter: ['class'], subtree: true });

    // Keyboard handler.
    navElement.addEventListener('keydown', (e) => {
        const currentItems = getItems();
        const currentIndex = currentItems.indexOf(document.activeElement);
        if (currentIndex === -1) return;

        let nextIndex;
        switch (e.key) {
            case 'ArrowDown':
                nextIndex = Math.min(currentIndex + 1, currentItems.length - 1);
                break;
            case 'ArrowUp':
                nextIndex = Math.max(currentIndex - 1, 0);
                break;
            case 'Home':
                nextIndex = 0;
                break;
            case 'End':
                nextIndex = currentItems.length - 1;
                break;
            default:
                return;
        }

        e.preventDefault();
        currentItems.forEach(item => item.setAttribute('tabindex', '-1'));
        currentItems[nextIndex].setAttribute('tabindex', '0');
        currentItems[nextIndex].focus();
    });

    // Update roving tabindex on focus (e.g. when user clicks an item).
    navElement.addEventListener('focusin', (e) => {
        const currentItems = getItems();
        if (currentItems.includes(e.target)) {
            currentItems.forEach(item => item.setAttribute('tabindex', '-1'));
            e.target.setAttribute('tabindex', '0');
        }
    });
}

/**
 * Sync aria-current="page" with NavLink's active class.
 * @param {HTMLElement} navElement
 */
function syncAriaCurrent(navElement) {
    const items = navElement.querySelectorAll('[role="menuitem"]');
    items.forEach(item => {
        if (item.classList.contains('active')) {
            item.setAttribute('aria-current', 'page');
        } else {
            item.removeAttribute('aria-current');
        }
    });
}
