function GetValueFromCache(key, keyOfValue) {
    key = key + "_CSCMaster";
    var cacheObj = localStorage.getItem(key);
    if (cacheObj == undefined)
        return '';
    if (keyOfValue == undefined)
        return JSON.parse(cacheObj);
    else
        return JSON.parse(cacheObj[keyOfValue]);
}

function AddIntoCache(key, value) {
    key = key + "_CSCMaster";
    localStorage.setItem(key, JSON.stringify(value));
}

function RemoveFromCache(key) {
    key = key + "_CSCMaster";
    localStorage.removeItem(key);
}

function getUrlQueryStrings() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function getUrlQueryString(sParam) {
    var sPageURL = window.location.search.substring(1),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
        }
    }
};

function SetRatings(name, ratings) {
    $('[name^=' + name + ']').removeClass("fas");
    for (let i = 0; i <= ratings; i++)
    {
        $('[name=ReviewPostRating' + i + ']').addClass("fas");
    }
    
};

function SetControlError(controlId, errorText, swapClassToError) {
    var _controlId = "#" + controlId;
    $("#divet_" + controlId).remove();
    $(_controlId).addClass('enable-controlaserror');
    $(_controlId).after("<div id = 'divet_" + controlId + "'><small id = 'et_" + controlId + "' class = 'enable-error-text'>" + errorText + "</small></div>");
    if (swapClassToError != "") {
        $(_controlId).removeClass(swapClassToError);
        $("#divet_" + controlId).addClass(swapClassToError);
    }
}

function RemoveControlError(controlId, swapClassToError) {
    var _controlId = "#" + controlId;
    $("#divet_" + controlId).remove();
    $(_controlId).removeClass('enable-controlaserror');
    if (swapClassToError != "") {
        $(_controlId).addClass(swapClassToError);
    }
}

function isNullOrEmpty(obj, propertyChain) {
    const properties = propertyChain.split('.');
    let currentObj = obj;
    for (let i = 0; i < properties.length; i++) {
        if (currentObj?.hasOwnProperty(properties[i])) {
            currentObj = currentObj[properties[i]];
        } else {
            console.log(`Property '${properties[i]}' is undefined.`);
            return true;
        }
    }
    if (typeof currentObj === 'string') {
        return currentObj.trim() === '';
    } else if (typeof currentObj === 'number') {
        return currentObj <= 0;
    } else if (Array.isArray(currentObj)) {
        return currentObj.length === 0;
    } else if (typeof currentObj === 'object') {
        return (currentObj === null || currentObj == undefined);
    } else 
        return true;
}
function printMe(elem) {
    var elements = elem.querySelectorAll('.rowtbl-h4');
    elements.forEach(function (el) {
        el.classList.remove('rowtbl-h4');
    });
    var domClone = elem.cloneNode(true);
    var printSection = document.getElementById("printSection");

    if (!printSection) {
        var printSection = document.createElement("div");
        printSection.id = "printSection";
        document.body.appendChild(printSection);
    }

    printSection.innerHTML = "";
    printSection.appendChild(domClone);
    window.print();
    printSection.removeChild(domClone);
}

function getTransactionDateRange() {
    var currentDate = new Date();
    var dateRange = [];
    if (!GetValueFromCache("bfpayroute"))
        currentDate.setMonth(currentDate.getMonth() - 1);

    if (currentDate.getDate() > 24)
        currentDate.setMonth(currentDate.getMonth() + 1);

    currentDate.setDate(1);
    for (var i = 0; i < 6; i++) {
        currentDate.setMonth(currentDate.getMonth() - (i > 0 ? 1 : i));
        var date = {};
        date.key = currentDate.getFullYear() + "-" + (currentDate.getMonth() + 1) + "-1";
        date.value = currentDate.getFullYear() + "-" + currentDate.toLocaleString('default', { month: 'short' });
        dateRange.push(date);
        if (i == 0 && GetValueFromCache("usertype") == "6")
            break;
    }
    return dateRange;
}

function ConvertToBlob(base64String, converToType) {
    const byteCharacters = atob(base64String);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);

    const blob = new Blob([byteArray], { type: converToType });
    return blob;
}

function OpenForPrint(blob) {
    const url = URL.createObjectURL(blob);
    const newWindow = window.open(url, '_blank');
    newWindow.onload = function () {
        newWindow.focus();
        newWindow.print();
        //newWindow.document.body.setAttribute('onafterprint', 'window.close()'); - Not working
        // Clean up
        URL.revokeObjectURL(url);
    };
}
function DownloadFile(blob, fileName) {
    var a = document.createElement('a');
    a.style.display = 'none';
    var url = window.URL.createObjectURL(blob);
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
}

function getLastDateOfMonth(date) {
    const givenDate = new Date(date);

    const currentMonth = givenDate.getMonth();
    const currentYear = givenDate.getFullYear();
    givenDate.setMonth(currentMonth + 1);

    // If the month overflowed, adjust the year
    if (givenDate.getMonth() !== (currentMonth + 1) % 12) {
        givenDate.setFullYear(currentYear + 1);
        givenDate.setMonth((currentMonth + 1) % 12);
    }

    if (givenDate.getDate() != 1)
        givenDate.setDate(25);
    else
        givenDate.setDate(0);

    return givenDate;
}

function roundOf(num, decimalPlaces) {
    const factor = Math.pow(10, decimalPlaces);
    return Math.round(num * factor) / factor;
}

function createGUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function initializeDatepickers() {
    $('.input-group.date').each(function () {
        const $group = $(this);
        const $input = $group.find('input');
        const $button = $group.find('button');

        $button.on('click', function () {
            $input.datepicker('show');
        });
    });
}

function setToDatePicker(controlName, isoDate) {
    $('#' + controlName).datepicker('setDate', new Date(new Date(isoDate).toLocaleString()));
}