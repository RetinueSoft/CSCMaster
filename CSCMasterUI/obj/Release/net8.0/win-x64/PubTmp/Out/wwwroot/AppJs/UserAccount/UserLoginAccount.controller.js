var myModule = angular.module('app');

myModule.controller('UserLoginAccountController', ['$scope', '$rootScope', 'UserAccountService', UserLoginAccountController]);

function UserLoginAccountController(binder, rootBinder, service) {
    binder.user = { name: '', mobile: '', password: ''};
    binder.login = login;
    binder.retrivePassword = retrivePassword;
    binder.restrictAadhar = function ($event) {
        var input = $event.target;
        input.value = input.value.replace(/[^0-9]/g, '').slice(0, 12);
    };

    function modalClose() {
        $('.modal').modal('hide');
    }

    function login() {
        var validationStatus = true;

        RemoveControlError("userMobileGroupLogin");
        RemoveControlError("userPasswordGroupLogin");
        if (isNullOrEmpty(binder.user, 'mobile')) {
            SetControlError("userMobileGroupLogin", "Mobile shouldn't be empty"); validationStatus = false;
        }
        if (isNullOrEmpty(binder.user, 'password')) {
            SetControlError("userPasswordGroupLogin", "Password shouldn't be empty"); validationStatus = false;
        }

        if (!validationStatus) {
            return;
        }

        var returnURL = getUrlQueryString("ReturnUrl");
        service.login(binder.user, function (result) {
            AddIntoCache("logintoken", result.accessToken);
            AddIntoCache("refreshtoken", result.refreshToken);
            AddIntoCache("curSymbole", result.settings.currencySymbole);
            AddIntoCache("dateTimeFormat", result.settings.dateTimeFormat);
            AddIntoCache("userData", result.user);
            fetch("/Home/StoreToken", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(result.accessToken)
            }).then(response => {
                if (response.ok) {
                    if (returnURL && returnURL !== "") {
                        window.location.href = returnURL;
                    } else
                        window.location.href = "/Dashboard?userRole=" + encodeURIComponent(result.user.role);
                } else {
                    alert("Token store failed.");
                }
            });
        });
    }
    function retrivePassword() {
        var validationStatus = true;

        RemoveControlError("userMobileGroupLogin");
        RemoveControlError("userAadharGroupLogin");

        if (isNullOrEmpty(binder.user, 'mobile')) {
            SetControlError("userMobileGroupLogin", "Mobile shouldn't be empty"); validationStatus = false;
        }
        if (isNullOrEmpty(binder.user, 'aadharNumber')) {
            SetControlError("userAadharGroupLogin", "Aadhar shouldn't be empty"); validationStatus = false;
        }
        if (!validationStatus) {
            return;
        }

        service.resetPassword(user, function (result) {
            modalClose();
        }, function (result) {
            $.each(result, function (key, value) {
                value = value.replace(/\n/g, "<br />");
                SetControlError("userMobileGroupReset", value);
            });
        });
    }
}
