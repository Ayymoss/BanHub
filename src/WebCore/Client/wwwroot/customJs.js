// Theme switching
let currentTheme = localStorage.getItem('theme') || 'dark';
const themeStyle = document.getElementById('theme-style');

function setTheme(theme) {
    if (theme === 'light') {
        themeStyle.href = 'css/material3-base.css';
    } else {
        themeStyle.href = 'css/material3-dark-base.css';
    }
}

setTheme(currentTheme);

window.toggleTheme = () => {
    if (currentTheme === 'dark') {
        currentTheme = 'light';
    } else {
        currentTheme = 'dark';
    }
    setTheme(currentTheme);
    localStorage.setItem('theme', currentTheme);
    return currentTheme;
};
