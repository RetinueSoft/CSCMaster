var myModule = angular.module('app');

myModule.controller('ItemController', ['$scope', 'PettyCashService', 'PopupModalService', ItemController]);

function ItemController(binder, service, popupModal) {
    binder.input = { };
    binder.tableApi = null;
    binder.listItems = listItems;
    binder.addItem = addItem;
    binder.editItem = editItem;

    binder.onPagedTableInit = function (api) {
        binder.tableApi = api;
    };
    function listItems(pageNumber, successFun) {
        service.featchItems(binder.input, function (result) {
            successFun(result);
        });
    }

    function addItem() {
        RemoveControlError("name");
        var validationStatus = true;

        if (isNullOrEmpty(binder.input, 'name')) {
            SetControlError("name", "Name should not be empty"); validationStatus = false;
        }

        if (!validationStatus)
            return;

        service.createItem(binder.input, function (result) {
            //FirstPage Refresh
        });
    };

    function editItem() {
        RemoveControlError("name");
        var validationStatus = true;

        if (isNullOrEmpty(binder.input, 'id')) {
            SetControlError("name", "Item not exist, id is empty"); validationStatus = false;
        }

        if (isNullOrEmpty(binder.input, 'name')) {
            SetControlError("name", "Name should not be empty"); validationStatus = false;
        }

        if (!validationStatus)
            return;

        service.editItem(binder.input, function (result) {
            //CurrentPage Refresh
        });
    };

}

