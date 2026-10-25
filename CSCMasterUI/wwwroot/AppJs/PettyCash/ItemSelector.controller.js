var myModule = angular.module('app');

myModule.controller('ItemSelectorController', ['$scope', '$rootScope', 'PettyCashService', ItemSelectorController]);

function ItemSelectorController(binder, rootBinder, service) {
    var itemFullList = [];
    binder.items = [];
    binder.input = {};
    binder.manage = false;
    binder.modalClose = modalClose;
    binder.selectItem = selectItem;
    binder.saveItem = saveItem;
    binder.editItem = editItem;
    binder.listItems = listItems;

    //$("#searchBoxES").on("keydown", function (event) {
    //    if (event.keyCode === 13) {
    //        event.preventDefault();
    //        $("#searchBtnES").click();
    //    }
    //});

    rootBinder.$on("ItemSelectorControllerModalInitial", function (init, param) {
        binder.input.transactionType = param.transactionType;
        listItems();
    });

    async function listItems() {
        binder.manage = false;
        binder.input.id = 0;
        if (!itemFullList || itemFullList.length == 0) 
            itemFullList = await getCustomersAsync('');
        binder.items = filterItems(itemFullList, binder.input.searchBy);
        //binder.$apply();
    }

    function getCustomersAsync(search) {
        return new Promise((resolve, reject) => {
            try {
                service.getItems(search, function (result) {
                    resolve(result || []);
                });
            } catch (e) {
                reject(e);
            }
        });
    }

    function filterItems(list, search) {
        search = (search || '').toLowerCase();
        return list.filter(x =>
            (x.name || '').toLowerCase().includes(search) &&
            x.transactionType == binder.input.transactionType
        );
    }

    
    function modalClose(selectedItem) {
        $('#ItemSelector').modal('hide');
        rootBinder.$emit("ItemSelectorControllerModalClosed", {
            selectedItem: selectedItem
        });
    }

    function selectItem(item) {
        modalClose(item);
    };

    function editItem(item) {
        binder.manage = true;
        binder.input.searchBy = item.name;
        binder.input.mrp = item.mrp;
        binder.input.id = item.id;
    }
    function saveItem(item) {
        if (!item || item.id == 0) {
            item = {};
            item.name = binder.input.searchBy;
            item.mrp = binder.input.mrp;
            item.transactionType = binder.input.transactionType;
            service.createItem(item, function (result) {
                itemFullList.push(result);
                rootBinder.$emit("ItemSelectorControllerItemAdded", { addedItem: result });
                listItems();
            });
        }
        else {
            item.name = binder.input.searchBy;
            item.mrp = binder.input.mrp;
            service.editItem(item, function (result) {
                const index = itemFullList.findIndex(x => x.id === result.id);
                itemFullList[index] = result;
                rootBinder.$emit("ItemSelectorControllerItemEdited", { editedItem: result });
                listItems();
            });
        }
    }
}