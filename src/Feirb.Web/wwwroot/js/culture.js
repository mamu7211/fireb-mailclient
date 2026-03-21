window.blazorCulture = {
    get: () => localStorage.getItem('BlazorCulture'),
    set: (value) => localStorage.setItem('BlazorCulture', value)
};
