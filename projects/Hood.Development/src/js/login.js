(function () {
    'use strict';

    var Login = /** @class */ (function () {
        function Login() {
            this.loadBackground();
        }
        Login.prototype.loginPage = function () {
        };
        Login.prototype.loadBackground = function () {
            if ($('#image-container').length > 0) {
                $.get($('#image-container').data('imageUrl'), this.renderImage.bind(this));
            }
        };
        Login.prototype.renderImage = function (images) {
            if (typeof images == "string") {
                $('#image-container').css({ 'background-image': "url(" + images + ")" });
                $('#image-credit').hide();
            }
            else {
                if (images.length > 0) {
                    $('#image-container').attr('title', images[0].description);
                    $('#image-container').css({ 'background-image': "url(" + images[0].urls.regular + ")" });
                    $('#image-credit').html("Image by <a href='" + images[0].user.links.html + "?utm_source=Hood%20CMS&utm_medium=referral'>" + images[0].user.name + "</a> on <a href='https://unsplash.com/?utm_source=Hood%20CMS&utm_medium=referral'>Unsplash</a>");
                }
                else {
                    $('#image-credit').hide();
                }
            }
        };
        return Login;
    }());
    new Login();

}());
//# sourceMappingURL=login.js.map
