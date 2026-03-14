(function () {
    'use strict';

    angular.module('app').service('UserAccountService', UserAccountService);
    UserAccountService.$inject = ['server'];

    function UserAccountService(server) {   
        this.login = login;
        this.resetPassword = resetPassword;
        this.logout = logout;
        this.getUserDetail = getUserDetail;

        function login(user, sucessFunc, failureFunc) {
            server.Post('/Auth/Login', { mobile: user.mobile, password: user.password, clientTimeOffset: (new Date()).getTimezoneOffset() }, sucessFunc, failureFunc);
        }
        function resetPassword(user, sucessFunc, failureFunc) {
            server.Post('/UserAccount/ResetPassword', { user: user }, sucessFunc, failureFunc);
        }
        function logout(sucessFunc) {
            server.Get('/Auth/Logout', sucessFunc);
        }
        function getUserDetail(sucessFunc) {
            server.Get('/User/UserDetail', sucessFunc);
        }
    }
})();