window.highlightCode = function (element, code) {
    if (element && window.hljs) {
        element.textContent = code;
        element.removeAttribute('data-highlighted');
        window.hljs.highlightElement(element);
    }
};
