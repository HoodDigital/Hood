let grecaptcha: any;

function hood__getReCaptcha(key: any, recaptchaId: string, action: any) {
    grecaptcha.ready(function () {
        grecaptcha.execute(key, { 'action': action }).then(function (token: any) {
            (<HTMLInputElement>document.getElementById(recaptchaId)).value = token;
        });
    });
}
