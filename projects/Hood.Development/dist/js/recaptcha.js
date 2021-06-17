var grecaptcha;
function hood__getReCaptcha(key, recaptchaId, action) {
    grecaptcha.ready(function () {
        grecaptcha.execute(key, { 'action': action }).then(function (token) {
            document.getElementById(recaptchaId).value = token;
        });
    });
}
