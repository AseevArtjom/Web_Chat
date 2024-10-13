document.addEventListener("DOMContentLoaded", function () {
    const themeToggleSwitch = document.getElementById("theme");
    const themeLink = document.getElementById("theme-link");

    const savedTheme = localStorage.getItem("theme");
    if (savedTheme) {
        themeLink.href = `/${savedTheme}-theme.css`;
        themeToggleSwitch.checked = savedTheme === "dark";
    }

    themeToggleSwitch.addEventListener("change", function () {
        const newTheme = themeToggleSwitch.checked ? "dark" : "light";
        themeLink.href = `/${newTheme}-theme.css`;
        localStorage.setItem("theme", newTheme);
    });
});
