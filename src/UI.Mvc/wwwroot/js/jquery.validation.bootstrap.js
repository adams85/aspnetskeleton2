// https://dotnetthoughts.net/how-to-use-bootstrap-style-validation-in-aspnet-core/
// https://blog.bitscry.com/2019/08/28/bootstrap-4-form-validation/

var settings = {
    validClass: "is-valid",
    errorClass: "is-invalid"
};
$.validator.setDefaults(settings);
$.validator.unobtrusive.options = settings;
