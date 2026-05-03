$(function () {
    $("#PasswordVisibilityButton").click(function () {
        const button = $(this);
        const passwordInput = button.parent().find("input");
        if (!passwordInput || passwordInput.length === 0) {
            return;
        }

        const isPassword = passwordInput.attr("type") === "password";
        passwordInput.attr("type", isPassword ? "text" : "password");

        const faIcon = button.find("i");
        if (faIcon && faIcon.length > 0) {
            faIcon.toggleClass("fa-eye-slash").toggleClass("fa-eye");
        }

        const symbolIcon = button.find(".nh-login-visibility-icon");
        if (symbolIcon && symbolIcon.length > 0) {
            symbolIcon.text(isPassword ? "visibility" : "visibility_off");
        }

        const showLabel = button.attr("data-show-label");
        const hideLabel = button.attr("data-hide-label");
        if (showLabel && hideLabel) {
            button.attr("aria-label", isPassword ? hideLabel : showLabel);
        }
    });
});
