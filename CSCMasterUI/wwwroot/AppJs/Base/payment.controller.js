var myModule = angular.module('app');

myModule.controller('PaymentController', ['$scope', '$rootScope', 'PettyCashService', PaymentController]);
function PaymentController(binder, rootBinder, masterService) {
    binder.payments = [];
    binder.updateCreditAmount = updateCreditAmount;
    binder.clearAll = function () { var totalAmt = binder.payments.reduce((sum, item) => sum + item.amount, 0); binder.payments.forEach(payment => payment.amount = 0); findUpdateCashAmount(totalAmt); };
    binder.totalAmount = function (payments) { return payments.reduce((sum, payment) => sum + payment.amount, 0); }
    var cashPaymentType = 0;
    function getAllPaymodes() {
        binder.payments = binder.sharedPaymentObject.payments;        
        masterService.getPaymodes(function (result) {
            if (binder.sharedPaymentObject.creditNeeded) {
                result.push({ id: 999, value:'Credit' });
            }
            if (binder.payments.length === 0) {
                binder.payments = result.map(x => ({
                    paymentType: x.id,
                    description:x.value,
                    amount: 0
                }));
            }
            cashPaymentType = result.find(x => x.value.toUpperCase() == 'CASH').id;
        });
    }
    getAllPaymodes();

    binder.$on('UpdateCreditAmount', function (event, data) {
        binder.sharedPaymentObject.paymentTotalAmount = data.newPaymentAmount;
        if (data.newPayments) {
            binder.sharedPaymentObject.payments = data.newPayments;
            binder.payments = binder.sharedPaymentObject.payments;
            if (binder.sharedPaymentObject.creditNeeded && !binder.payments.some(payment => payment.paymentType === 999)) {
                binder.payments.push({ paymentType: Number('999'), amount: 0 });
            }
            //updateCreditAmount(binder.payments);

        }
        findPaymodeAndCreditAmount();
    });
    binder.$on('UpdateCashAmount', function (event, data) {
        findUpdateCashAmount(data.newPaymentAmount);
    });
    binder.$on('ClearPayments', function (event, data) {
        binder.payments.forEach(x => { x.amount = 0; });
        binder.clearAll();
    });
    function findUpdateCashAmount(newPaymentAmount) {
        binder.sharedPaymentObject.paymentTotalAmount = newPaymentAmount;
        var payment = binder.payments.find(x => x.paymentType == cashPaymentType);
        if (payment) {
            payment.amount = newPaymentAmount;
        }
        findPaymodeAndCreditAmount();
    }

    function findPaymodeAndCreditAmount() {
        updateCreditAmount(binder.payments.find(x => x.payment != 999)); //Find will retrun firstOrDefault
    }
    function updateCreditAmount(paymode) {
        binder.sharedPaymentObject.payments = binder.payments;
        if (binder.sharedPaymentObject.creditNeeded) {
            if (paymode.paymentType != 999) {
                var paidAmount = binder.payments.reduce((sum, payment) => {
                    if (payment.paymentType != 999) 
                        return sum + payment.amount;
                    return sum;
                }, 0);
                var creditAmount = Number(binder.sharedPaymentObject.paymentTotalAmount || 0) - paidAmount;
                creditAmount = creditAmount < 0 ? 0 : creditAmount;
                binder.payments.find(x => x.paymentType == 999).amount = creditAmount;
            }
        }
    }
}
