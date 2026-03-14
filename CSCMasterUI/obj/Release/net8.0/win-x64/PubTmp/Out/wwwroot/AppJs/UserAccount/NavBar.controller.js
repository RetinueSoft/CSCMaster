var myModule = angular.module('app');

myModule.controller('NavBarController', ['$scope', 'UserAccountService', NavBarController]);

function NavBarController(binder, service) {
    binder.user = GetValueFromCache("userData");
    binder.alertMessage = [/*{ id: 1, title: "Welcome", text: "Welcome back dfgdfgd gdfgdgdgdgd g" }, { id: 2, title:"Payment",text: "Payment is due" }*/];
    binder.toggleMenuNavigator = toggleMenuNavigator;
    binder.toggleAlert = toggleAlert;
    binder.removeAlert = removeAlert;
    binder.logout = logout;
    if (!binder.user) {
        //window.location.href = '/Home/Login';
    }
    
    function logout() {
        RemoveFromCache('logintoken');
        RemoveFromCache('refreshtoken');
        RemoveFromCache('userData');
        binder.user = null;
        window.location.href = '/Home/Login';
    };

    function toggleMenuNavigator() {
        $("#menuNavigator").modal("show");
    }
    function toggleAlert() {
        $("#menuNavigator").modal("show");
    }
    function removeAlert(msg) {
        binder.alertMessage = binder.alertMessage.filter(l => l.id !== msg.id);
    }
}
