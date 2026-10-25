var myModule = angular.module('app');

myModule.controller('CustomerSelectorController', ['$scope', '$rootScope', 'PettyCashService', CustomerSelectorController]);

function CustomerSelectorController(binder, rootBinder, service) {
    var CustomerFullList = [];
    binder.customers = [];
    binder.input = { contactNumber: [] };
    binder.manage = false;
    binder.mobileNumberTypes = [];

    binder.modalClose = modalClose;
    binder.selectCustomer = selectCustomer;
    binder.saveCustomer = saveCustomer;
    binder.editCustomerClicked = editCustomerClicked;
    binder.listCustomers = listCustomers;
    binder.addPhone = addPhone;
    binder.removePhone = removePhone;
    binder.cancelManage = function () {
        binder.manage = false;
        binder.input.id = 0;

        RemoveControlError("name");
        RemoveControlError("caddress");
        RemoveControlError("cmobile");
    }

    //$("#searchBoxES").on("keydown", function (event) {
    //    if (event.keyCode === 13) {
    //        event.preventDefault();
    //        $("#searchBtnES").click();
    //    }
    //});
    loadTransactionTypes();
    function loadTransactionTypes() {
        service.getMobileNumberTypes(function (result) {
            binder.mobileNumberTypes = result.map(function (x) {
                return {
                    id: x.id,
                    displayName: x.value
                };
            });
        });
    }
    function addPhone(type, number) {
        RemoveControlError("mobileType");
        RemoveControlError("cmobile");

        var validationStatus = true;
        number = number.toString().trim();

        if (!type) {
            SetControlError("mobileType", "Phone type is required"); validationStatus = false;
        }

        if (!number) {
            SetControlError("cmobile", "Phone number is required"); validationStatus = false;
        }

        if (number.length != 10) {
            SetControlError("cmobile", "Enter valid mobile number"); validationStatus = false;
        }

        var alreadyExists = binder.input.contactNumber.some(function (x) {
            return x.number === number;
        });

        if (alreadyExists) {
            SetControlError("cmobile", "Number already exists"); validationStatus = false;
        }

        if (!validationStatus)
            return;

        binder.input.contactNumber.push({
            type: type,
            number: number
        });
    }
    function removePhone(item) {
        binder.input.contactNumber = binder.input.contactNumber.filter(function (x) {
            return !(x === item);
        });
    }

    rootBinder.$on("CustomerSelectorControllerModalInitial", function (init, param) {
        //binder.input.searchBy = "";
        listCustomers();
    });
    rootBinder.$on("SelectDefaultCustomer", function (init, param) {
        getCustomersAsync('').then(function (result) {
            var selectedCustomer = result.find(x => x.id == 1);
            modalClose(selectedCustomer);
        });
    });

    async function listCustomers() {
        binder.cancelManage();

        if (!CustomerFullList || CustomerFullList.length == 0) 
            CustomerFullList = await getCustomersAsync('');

        CustomerFullList = CustomerFullList.map(function (customer) {
            customer.displayContactNumber = (customer.contactNumber || [])
                .map(function (x) {
                    return x.number;
                })
                .join(", ");
            return customer;
        });

        binder.customers = filterCustomers(CustomerFullList, binder.input.searchBy);
        binder.$apply();
    }

    function getCustomersAsync(search) {
        return new Promise((resolve, reject) => {
            try {
                service.getCustomers(search, function (result) {
                    resolve(result || []);
                });
            } catch (e) {
                reject(e);
            }
        });
    }

    function filterCustomers(list, search) {
        search = (search || '').toLowerCase();
        return list.filter(function (x) {

            var nameMatch =
                (x.name || '')
                    .toLowerCase()
                    .includes(search);

            var numberMatch =
                (x.contactNumber || []).some(function (c) {

                    return (c.number || '')
                        .toString()
                        .includes(search);

                });

            return nameMatch || numberMatch;

        });
    }

    
    function modalClose(selectedCustomer) {
        $('#CustomerSelector').modal('hide');
        rootBinder.$emit("CustomerSelectorControllerModalClosed", {
            selectedItem: selectedCustomer
        });
    }

    function selectCustomer(Customer) {
        modalClose(Customer);
    };

    function editCustomerClicked(Customer) {
        binder.manage = true;
        binder.input.searchBy = Customer.name;
        binder.input.address = Customer.address;
        binder.input.contactNumber = Customer.contactNumber.map(function (x) {
            return {
                type: binder.mobileNumberTypes.find(n => n.id == x.type),
                number: x.number
            };
        });
        binder.input.id = Customer.id;
    }
    function saveCustomer(customer) {
        var validationStatus = true;
        RemoveControlError("name");
        RemoveControlError("caddress");
        RemoveControlError("cmobile");

        if (isNullOrEmpty(binder.input, 'searchBy')) {
            SetControlError("name", "Name shouldn't be empty"); validationStatus = false;
        }
        if (isNullOrEmpty(binder.input, 'address')) {
            SetControlError("caddress", "Address shouldn't be empty"); validationStatus = false;
        }
        if (isNullOrEmpty(binder.input, 'contactNumber') || binder.input.contactNumber.length <= 0) {
            SetControlError("cmobile", "Mobile shouldn't be empty"); validationStatus = false;
        }
        var primaryExist = binder.input.contactNumber.some(function (x) {
            return x.type.id === 1 || x.type.id === 2;
        });
        if (!primaryExist) {
            SetControlError("cmobile", "Primary Mobile number not exit"); validationStatus = false;
        }

        var existWith = getExistingCustomerNames(binder.input.contactNumber.filter(x => x.type.id === 1 || x.type.id === 2));
        if (existWith.length > 0) {
            alert("Primary Mobile number already exit for '" + existWith + "'. Cannot add"); validationStatus = false;
        }
        if (!validationStatus)
            return;

        if (!customer || isNullOrEmpty(customer, 'id') || customer.id == 0)
            createCustomer();
        else
            editCustomer(customer);
        
    }
    function createCustomer() {
        Customer = {};
        Customer.name = binder.input.searchBy;
        Customer.address = binder.input.address;
        Customer.contactNumber = binder.input.contactNumber.map(function (x) {
            return {
                Type: x.type.id,
                Number: x.number
            };
        });

        service.createCustomer(Customer, function (result) {
            CustomerFullList.push(result);
            listCustomers();
        });
    }
    function editCustomer(customer) {
        customer.name = binder.input.searchBy;
        customer.address = binder.input.address;
        customer.contactNumber = binder.input.contactNumber.map(function (x) {
            return {
                Type: x.type.id,
                Number: x.number
            };
        });
        service.editCustomer(customer, function (result) {
            const index = CustomerFullList.findIndex(x => x.id === result.id);
            CustomerFullList[index] = result;
            listCustomers();
        });
    }
    function getExistingCustomerNames(numberList) {

        if (!numberList || !numberList.length)
            return "";

        var matchedNames = [];

        CustomerFullList.forEach(function (customer) {

            var isMatched = (customer.contactNumber || []).some(function (mobile) {

                return (mobile.type === 1 || mobile.type === 2)
                    && numberList.some(function (n) {

                        return n
                            && n.number
                            && mobile.number
                            && n.number.toString().trim() === mobile.number.toString().trim();

                    });

            });

            if (isMatched && customer.name) {

                if (matchedNames.indexOf(customer.name) === -1)
                    matchedNames.push(customer.name);

            }

        });

        return matchedNames.join(", ");
    }
}