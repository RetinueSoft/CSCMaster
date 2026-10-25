var myModule = angular.module('app');

myModule.controller('CustomerController', ['$scope', 'PettyCashService', 'PopupModalService', CustomerController]);

function CustomerController(binder, service, popupModal) {
    binder.input = {};
    binder.tableApi = null;
    binder.listCustomers = listCustomers;
    binder.addCustomer = addCustomer;
    binder.editCustomer = editCustomer;

    binder.onPagedTableInit = function (api) {
        binder.tableApi = api;
    };
    function listCustomers(pageNumber, successFun) {
        service.featchCustomers(binder.input, function (result) {
            successFun(result);
        });
    }

    function addCustomer() {
        RemoveControlError("name");
        var validationStatus = true;

        if (isNullOrEmpty(binder.input, 'name')) {
            SetControlError("name", "Name should not be empty"); validationStatus = false;
        }

        if (!validationStatus)
            return;

        service.createCustomer(binder.input, function (result) {
            //FirstPage Refresh
        });
    };

    function editCustomer() {
        RemoveControlError("name");
        var validationStatus = true;

        if (isNullOrEmpty(binder.input, 'id')) {
            SetControlError("name", "Customer not exist, id is empty"); validationStatus = false;
        }

        if (isNullOrEmpty(binder.input, 'name')) {
            SetControlError("name", "Name should not be empty"); validationStatus = false;
        }

        if (!validationStatus)
            return;

        service.editCustomer(binder.input, function (result) {
            //CurrentPage Refresh
        });
    };

}

