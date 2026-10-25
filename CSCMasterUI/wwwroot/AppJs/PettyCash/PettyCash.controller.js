var myModule = angular.module('app');

myModule.controller('PettyCashController', ['$scope', '$rootScope', 'PettyCashService', 'PopupModalService', PettyCashController]);

function PettyCashController(binder, rootBinder, service, popupModal) {
    binder.activity = 'ADD';
    binder.input = { date: getTodayForPicker(), items: [], notes: '', transactionType: 1 };
    binder.listInput = { fdate: getTodayForPicker(), tdate: getTodayForPicker() };
    binder.header = { };
    binder.transactionType = [];
    binder.paymodes = [];
    binder.accountTrans = {};
    binder.addItem = addItem;
    binder.deleteItem = deleteItem;
    binder.save = save;
    binder.sharedPaymentObject = { creditNeeded: true, payments: [] };
    binder.toggleItemSelector = toggleItemSelector;
    binder.toggleCustomerSelector = toggleCustomerSelector;

    binder.manageListItems = manageListItems;
    binder.manageTableApi = null;
    binder.onManagePagedTableInit = function (api) {
        binder.manageTableApi = api;
    };
    binder.listItems = listItems;
    binder.tableApi = null;
    binder.onPagedTableInit = function (api) {
        binder.tableApi = api;
    };

    binder.clearHeader = function () {
        binder.header.item = {};
        binder.header.mrp = '';
        binder.header.quantity = '';
        binder.header.discount = '';
        binder.header.itemSearchStr = '';
        binder.header.notes = '';
        $("#itemListInput").focus();
        binder.$applyAsync();
    };
    binder.clearPage = function (activity) {
        if (activity !== undefined && activity !== '')
            binder.activity = activity;
        else
            binder.activity = 'ADD';
        if (binder.activity == 'ADD') {
            if (binder.manageTableApi)
                binder.manageTableApi.refresh();
            $("#itemListInput").focus();
        }
        
        binder.clearHeader();
        binder.input = { date: getTodayForPicker(), items: [], notes: '', transactionType: 1 };
        binder.input.finalAmount = parseFloat(binder.itemsSum('price').toFixed(2));
        rootBinder.$broadcast('UpdateCashAmount', { newPaymentAmount: binder.input.finalAmount });
        rootBinder.$broadcast('ClearPayments');

        rootBinder.$broadcast("SelectDefaultCustomer", {});
        saveEntered = false;
    };
    binder.showList = function () {
        binder.activity = 'LIST';

        if (refreshListNeeded)
            binder.tableApi.refresh();
    };
    binder.itemsSum = function (field) {
        return (binder.input.items || []).reduce(function (sum, x) {
            return sum + (parseFloat(x[field]) || 0);
        }, 0);
    };    
    binder.listHeader = {};
    let refreshListNeeded = false;
    binder.editClicked = editClicked;
    binder.deleteAccount = deleteAccount;
    binder.getListHeader = getListHeader;
    binder.findSelectedItem = findSelectedItem;

    
    function getListHeader(pageNumber, successFun) {
        service.getPettyCashListHeader({ fromDate: binder.listInput.fdate, toDate: binder.listInput.tdate }, function (result) {
            binder.listHeader = result;
        });
    }
    function listItems(pageNumber, successFun) {
        service.getPettyCashTrans({ fromDate: binder.listInput.fdate, toDate: binder.listInput.tdate, pageIndex: pageNumber }, function (result) {
            result.pagedList.forEach(y => {
                y.items.forEach(x => {
                    x.price = (x.mrp * x.quantity) - x.discount;
                })
            });
            successFun(result);
        });
    }
    function manageListItems(pageNumber, successFun) {
        service.getPettyCashTrans({ fromDate: getTodayForPicker(), toDate: getTodayForPicker(), pageIndex: pageNumber }, function (result) {
            result.pagedList.forEach(y => {
                y.items.forEach(x => {
                    x.price = (x.mrp * x.quantity) - x.discount;
                })
            });
            successFun(result);
        });
    }
    function toggleItemSelector() {
        $("#ItemSelector").modal('show');
        rootBinder.$emit("ItemSelectorControllerModalInitial", { transactionType: binder.input.transactionType });
    }
    rootBinder.$on("ItemSelectorControllerModalClosed", function (init, param) {
        if (Object.keys(param['selectedItem']).length === 0)
            return;

        binder.header.item = param['selectedItem'];
        setDefaultSelectedItemHeader();
    });
    function toggleCustomerSelector() {
        $("#CustomerSelector").modal('show');
        rootBinder.$emit("CustomerSelectorControllerModalInitial", {});
    }
    rootBinder.$on("CustomerSelectorControllerModalClosed", function (init, param) {
        if (Object.keys(param['selectedItem']).length === 0)
            return;
        binder.input.customer = param['selectedItem'];
    });

    binder.onPagedTableInit = function (api) {
        binder.tableApi = api;
    };

    loadTransactionTypes();

    function loadTransactionTypes() {
        service.getTransactionTypes(function (result) {
            binder.transactionType = result;
        });
    }

    function editClicked(item) {
        binder.input = item;
        binder.activity = 'EDIT';
        binder.input.items.map(x => x.price = (x.mrp * x.quantity) - x.discount);
        binder.input.finalAmount = parseFloat(binder.itemsSum('price').toFixed(2));
        rootBinder.$broadcast('UpdateCreditAmount', { newPaymentAmount: binder.input.finalAmount, newPayments: binder.input.payment });
    }

    function addItem() {
        let validationStatus = true;
        RemoveControlError("mrp");
        RemoveControlError("qty");
        RemoveControlError("discount");
        RemoveControlError("item");
        RemoveControlError("fprice");
        if (isNullOrEmpty(binder.header, 'item') || Object.keys(binder.header.item).length == 0) {
            SetControlError("item", "Item is required"); validationStatus = false;
        }
        if (isNullOrEmpty(binder.header, 'mrp')) {
            SetControlError("mrp", "MRP is required"); validationStatus = false;
        }
        if (isNullOrEmpty(binder.header, 'quantity')) {
            SetControlError("qty", "Qty is required"); validationStatus = false;
        }
        if (binder.header.quantity <= 0) {
            SetControlError("qty", "Qty must be greater than 0"); validationStatus = false;
        }
        if (binder.header.mrp <= 0) {
            SetControlError("mrp", "MRP must be greater than 0"); validationStatus = false;
        }

        if (binder.header.discount < 0) {
            SetControlError("discount", "Discount cannot be negative"); validationStatus = false;
        }


        let isDuplicate = binder.input.items.some(function (i) {
            return i.item && binder.header.item && i.item.id === binder.header.item.id;
        });

        if (isDuplicate) {
            SetControlError("item", "Item already added"); validationStatus = false;
        }

        binder.header.price = (binder.header.mrp * binder.header.quantity) - binder.header.discount;
        if (binder.header.price < 0) {
            SetControlError("fprice", "Price cannot be negative"); validationStatus = false;
        }

        if (!validationStatus) return;

        binder.input.items.push(angular.copy(binder.header));

        binder.input.finalAmount = parseFloat(binder.itemsSum('price').toFixed(2));
        rootBinder.$broadcast('UpdateCashAmount', { newPaymentAmount: binder.input.finalAmount });
        binder.clearHeader();
    }

    function deleteItem(item) {
        binder.input.items = binder.input.items.filter(function (i) {
            return i.item !== item.item;
        });
        binder.input.finalAmount = parseFloat(binder.itemsSum('price').toFixed(2));
        rootBinder.$broadcast('UpdateCashAmount', { newPaymentAmount: binder.input.finalAmount });
    }

    var saveEntered = false;
    function save() {
        if (saveEntered)
            return;

        saveEntered = true;
        var validationStatus = true;
        RemoveControlError("item");
        RemoveControlError("customer");
        RemoveControlError("payment");

        if (isNullOrEmpty(binder.input, 'items') || binder.input.items.length == 0) {
            SetControlError("item", "Items shouldn't be empty"); validationStatus = false;
        }
        if (isNullOrEmpty(binder.input, 'customer') || isNullOrEmpty(binder.input.customer, 'id') || binder.input.customer.id == 0) {
            SetControlError("customer", "Customer shouldn't be empty"); validationStatus = false;
        }
        var paymentAmount = (binder.sharedPaymentObject.payments || []).reduce(function (sum, x) {
                                return sum + (parseFloat(x['amount']) || 0);
                            }, 0);
        if (binder.input.finalAmount && binder.input.finalAmount < paymentAmount) {
            SetControlError("payment", "Payment cannot be grater than " + binder.input.finalAmount); validationStatus = false;
        }
        if (!validationStatus)
            return;

        if (binder.activity == "ADD")
            createAccount();
        else
            editAccount();

    }
    
    function createAccount() {
        binder.input.payment = binder.sharedPaymentObject.payments;
        binder.input.adjustments = [];
        service.createAccount(binder.input, function (result) {
            popupModal.success("Saved successfully!").then(function () {
                $("#itemListInput").focus();
            });
            refreshListNeeded = true;
            binder.clearPage();
        });
    }
    function editAccount(item) {
        binder.input.payment = binder.sharedPaymentObject.payments;
        binder.input.adjustments = [];

        service.editAccount(binder.input, function (result) {
            popupModal.success("Saved successfully!");
            refreshListNeeded = true;
            binder.clearPage("LIST");
        });
    }
    function deleteAccount(item) {
        if (!confirm("Are you sure want to delete this record?"))
            return;
        service.deleteAccount(item.id, function (result) {
            binder.tableApi.refresh();
        });
    }

    binder.items = [];
    getItems();
    function getItems() {
        service.getItems('', function (result) {
            binder.items = result;
        });
    }

    rootBinder.$on("ItemSelectorControllerItemAdded", function (init, param) {
        if (Object.keys(param['addedItem']).length === 0)
            return;
        binder.items.push(param.addedItem);
    });

    rootBinder.$on("ItemSelectorControllerItemEdited", function (init, param) {
        if (Object.keys(param['editedItem']).length === 0)
            return;

        const index = binder.items.findIndex(x => x.id === param.editedItem.id);
        binder.items[index] = param.editedItem;
    });

    function findSelectedItem() {
       binder.header.item = binder.items.find(
           x => x.name === binder.input.itemSearchStr
        );
        if (!binder.header.item)
            return;

        setDefaultSelectedItemHeader();
    };

    binder.onKeyDownForAddItem = function (event) {
        if (event.key === 'Enter') {
            addItem();
        }
    };

    binder.onItemKeyDown = function (event) {
        if (event.key === 'Enter') {
            event.stopPropagation();

            var search = (binder.header.itemSearchStr || '').toLowerCase();
            var firstMatch = binder.items.find(function (x) {
                return x.name.toLowerCase().includes(search);
            });
            if (firstMatch && binder.header.item == firstMatch) {
                addItem();
            }
            if (firstMatch) {
                binder.header.item = firstMatch;
                binder.header.itemSearchStr = firstMatch.name;
                setDefaultSelectedItemHeader();
            }
            event.preventDefault();
        }
    };

    binder.onKeyDown = function (event) {

        if ((event.ctrlKey || event.metaKey) && event.keyCode === 83) {
            event.stopPropagation();
            event.preventDefault();

            binder.save();
        }
    };

    function setDefaultSelectedItemHeader() {
        binder.header.mrp = binder.header.item.mrp;
        binder.header.quantity = 1;
        binder.header.discount = 0;
    }
}


