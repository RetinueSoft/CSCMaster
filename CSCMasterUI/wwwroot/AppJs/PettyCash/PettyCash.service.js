(function () {
    'use strict';

    angular.module('app').service('PettyCashService', PettyCashService);
    PettyCashService.$inject = ['server'];

    function PettyCashService(server) {   
        this.getCustomers = getCustomers;
        this.getItems = getItems;
        this.getPettyCashTrans = getPettyCashTrans;
        this.getPettyCashListHeader = getPettyCashListHeader;
        this.getTransactionTypes = getTransactionTypes;
        this.getPaymodes = getPaymodes;
        this.getMobileNumberTypes = getMobileNumberTypes;

        this.createItem = createItem;
        this.editItem = editItem;
        this.createCustomer = createCustomer;
        this.editCustomer = editCustomer;
        this.createAccount = createAccount;
        this.editAccount = editAccount;
        this.deleteAccount = deleteAccount;


        function getCustomers(searchBy, sucessFunc, failureFunc) {
            server.Get('/PettyCash/Customers?searchBy=' + encodeURIComponent(searchBy), sucessFunc, failureFunc);
        }
        function getItems(searchBy, sucessFunc, failureFunc) {
            server.Get('/PettyCash/Items?searchBy=' + encodeURIComponent(searchBy), sucessFunc, failureFunc);
        }
        function getPettyCashTrans(input, sucessFunc, failureFunc) {
            server.Post('/PettyCash/All', input, sucessFunc, failureFunc);
        }
        function getPettyCashListHeader(input, sucessFunc, failureFunc) {
            server.Post('/PettyCash/ListHeader', input, sucessFunc, failureFunc);
        }
        function getTransactionTypes(sucessFunc, failureFunc) {
            server.Get('/PettyCash/TransactionTypes', sucessFunc, failureFunc);
        }
        function getPaymodes(sucessFunc, failureFunc) {
            server.Get('/PettyCash/Paymodes', sucessFunc, failureFunc);
        }
        function getMobileNumberTypes(sucessFunc, failureFunc) {
            server.Get('/PettyCash/MobileNumberTypes', sucessFunc, failureFunc);
        }

        function createItem(item, sucessFunc, failureFunc) {
            server.Post('/PettyCash/CreateItem', item , sucessFunc, failureFunc);
        }
        function editItem(item, sucessFunc, failureFunc) {
            server.Put('/PettyCash/EditItem', item, sucessFunc, failureFunc);
        }
        function createCustomer(customer, sucessFunc, failureFunc) {
            server.Post('/PettyCash/CreateCustomer', customer, sucessFunc, failureFunc);
        }
        function editCustomer(customer, sucessFunc, failureFunc) {
            server.Put('/PettyCash/EditCustomer', customer, sucessFunc, failureFunc);
        }
        function createAccount(account, sucessFunc, failureFunc) {
            server.Post('/PettyCash/Create', account, sucessFunc, failureFunc);
        }
        function editAccount(account, sucessFunc, failureFunc) {
            server.Post('/PettyCash/Edit', account, sucessFunc, failureFunc);
        }
        function deleteAccount(accountId, sucessFunc, failureFunc) {
            server.Delete('/PettyCash/Delete?accountId=' + encodeURIComponent(accountId), {}, sucessFunc, failureFunc);
        }
    }
})();