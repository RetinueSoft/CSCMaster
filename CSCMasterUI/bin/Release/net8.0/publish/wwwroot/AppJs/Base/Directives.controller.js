var myModule = angular.module('app');

myModule.directive("txtgrp", function () {
    return {
        restrict: "E",
        scope: {
            id: "@",
            label: "@",
            placeholder: "@",
            model: "=",
            maxlength: "@",
            required: "@",
            cdivcls: "@",
            cls: "@",
            code: "@",
            mt: "@",
            keyup:"&?"
        },
        template: `
        <div class="input-group">
            <div class="input-group-prepend">
                <span class="input-group-text" ng-class="{\'rem6dot5\': !isOverflow}">{{displayLabel}}</span>
            </div>
            <input id="{{id}}" placeholder="{{displayPlaceholder}}" class="form-control"
                   type="text" maxlength="{{maxlength}}" ng-model="model"
                   ng-keypress="filterInput($event)"
                   ng-keyup="onKeyUp($event)"></input>
        </div>`,
        link: applyTextFilters
    };
});
myModule.directive("numgrp", function () {
    return {
        restrict: "E",
        scope: {
            id: "@",
            label: "@",
            placeholder: "@",
            model: "=",
            maxlength: "@",
            required: "@",
            cdivcls: "@",
            cls: "@",
            onenter: "&",
            mt: "@",
            inline: "@",
            change: "&"
        },
        controller: function ($scope, $element, $timeout) {
            const input = $element.find('input');
            input.on('input', function () {
                const regex = new RegExp(`^(\\d{0,${$scope.maxlength}}(?:\\.\\d{0,2})?).*$`);
                const cleanValue = this.value
                    .replace(/[^0-9.]/g, '')
                    .replace(/(\..*?)\./g, '$1')
                    .replace(regex, '$1');

                if (cleanValue !== this.value) {
                    this.value = cleanValue;
                }

                $scope.$applyAsync(() => {
                    $scope.model = cleanValue;
                });
            });

            $scope.onChange = function () {
                $timeout(function () { if ($scope.change) $scope.change(); }, 100);
            };
        },
        template: `
        <div class="input-group" ng-show="!sameline">
            <div class="input-group-prepend">
                <span class="input-group-text" ng-class="{\'rem6dot5\': !isOverflow}">{{displayLabel}}</span>
            </div>
            <input id="{{id}}" placeholder="{{displayPlaceholder}}" class="form-control"
                   type="text" ng-model="model"
                   ng-keydown="onEnterKey($event)"
                   oninput="this.value = this.value.replace(/[^0-9]/g, '')"></input>
        </div>

        <input ng-show="sameline" id="{{id}}" placeholder="{{displayPlaceholder}}" class="form-control"
                   type="text" ng-model="model"
                   ng-keydown="onEnterKey($event)"
                   ng-change="onChange()"></input>`,
        link: applyTextFilters
    };
});
myModule.directive("mobilegrp", function () {
    return {
        restrict: "E",
        scope: {
            id: "@",
            label: "@",
            placeholder: "@",
            model: "=",
            required: "@",
            cdivcls: "@",
            cls: "@",
            mt: "@",
            inline: "@"
        },
        transclude: true,
        template: `
         <div class="input-group">
            <div class="input-group-prepend" ng-show="!sameline">
                <span class="input-group-text rem6dot5">{{displayLabel}}</span>
            </div>
            <input id="{{id}}" placeholder="{{displayPlaceholder}}" class="form-control"
                   type="text" maxlength="10" ng-model="model"
                   oninput="this.value = this.value.replace(/[^0-9]/g, '').slice(0, 10)"></input>
            <div class="input-group-postpend">
                <div ng-transclude class="mt-0" style="height: 100%;"></div>
            </div>
        </div>`,
        link: applyTextFilters
    };
});
myModule.directive("passgrp", function () {
    return {
        restrict: "E",
        scope: {
            id: "@",
            label: "@",
            placeholder: "@",
            model: "=",
            required: "@",
            cdivcls: "@",
            cls: "@",
            onenter: "&",
            mt: "@"
        },
        template: `
        <div class="input-group">
            <div class="input-group-prepend">
                <span class="input-group-text rem6dot5">{{displayLabel}}</span>
            </div>
            <input type="password" class="form-control" id="{{id}}" 
                   placeholder="{{displayPlaceholder}}" ng-model="model"
                   ng-keydown="onEnterKey($event)">
        </div>`,
        link: applyTextFilters
    };
});
myModule.directive("chkgrp", function () {
    return {
        restrict: "E",
        scope: {
            id: "@",
            label: "@",
            placeholder: "@",
            model: "=",
            required: "@",
            cdivcls: "@",
            cls: "@",
            mt: "@",
            change: "&",
            disabled: "=",
            checked: "<"
        },
        template: `
        <div class="d-flex align-items-center">
            <label class="me-2">{{ displayLabel }}</label>
            <input id="{{id}}" type="checkbox" ng-model="model"  ng-change="change({newVal: model})"  ng-disabled="disabled" />
        </div>`,
        link: commonLink
    };
});
myModule.directive("radgrp", function () {
    return {
        restrict: "E",
        scope: {
            id: "@",
            label: "@",
            placeholder: "@",
            name: "@",
            model: "=",
            value: "<",
            cdivcls: "@",
            cls: "@",
            mt: "@",
            change: "&",
            disabled: "=",
            required: "@",
            checked: "<"
        },
        template: `
        <div class="d-flex align-items-center">
            <label class="me-2">{{ displayLabel }}</label>
            <input id="{{id}}" type="radio" name="{{name}}" ng-model="model" ng-value="value" ng-change="change()" ng-disabled="disabled" ng-checked="checked"/>
        </div>`,
        link: commonLink
    };
});
myModule.directive("cmbgrp", function () {
    return {
        restrict: "E",
        scope: {
            id: "@",
            label: "@",
            model: '=',
            display: '=',
            list: "=",
            required: "@",
            cdivcls: "@",
            cls: "@",
            mt: "@",
            inline: "@",
            api: '=?',
            changed: '&'
        },
        transclude: true,
        controller: function ($scope, $element, $timeout) {
            var mtVal = $scope.mt || "mt-4";
            $timeout(function () {
                const firstDiv = Array.from($element[0].children).find(el => el.tagName === "DIV");
                if (firstDiv)
                    angular.element(firstDiv).addClass(mtVal);
                else
                    $element.addClass(mtVal);

                if ($scope.modelMap && $scope.modelMap.length > 0 && !$scope.model) {
                    $scope.setModel($scope.modelMap[0].original);
                }
            });

            $scope.setModel = function (item) {
                $timeout(function () {
                    $scope.modelTemp = $scope.modelMap.find(x => x.original === item);
                    $scope.model = item;
                    $timeout(function () { $scope.changed(); }, 20);
                }, 20);
            }
            $scope.api = {
                setModel: $scope.setModel
            };

            
            $timeout(function () { $scope.$emit($scope.id+':ready', { api: $scope.api }); }, 50);
        },
        template: `
        <div class="input-group">
            <div class="input-group-prepend" ng-if="!sameline">
                <span class="input-group-text" ng-class="{'rem6dot5': !isOverflow}">
                    {{label}}
                </span>
            </div>
            <select style="flex: 1;" ng-model="modelTemp"
                    ng-class="{'form-control form-select': !sameline, 'inlineCombo flex-fill': sameline}"
                    ng-options="mm as mm.displayName for mm in modelMap"
                    ng-disabled="disabled">
            </select>
        </div>
        <span ng-transclude class="mt-0"></span>`,
        link: applyComboFilters
    };
});

myModule.directive("scbtngrp", function () {
    return {
        restrict: "E",
        scope: {
            id: "@",
            label: "@",
            sclick: "&",
            cclick:"&",
            disabled: "=",
            cdivcls: "@",
            cls: "@",
            mt: "@"
        },
        template: `
        <div class="d-flex align-items-center">
            <button class="btn btn-primary" ng-click="sclick()" ng-disabled="disabled">
                {{label}}
            </button>
            <button class="ms-1 btn btn-primary" ng-click="cclick()" ng-disabled="disabled">
                Cancel
            </button>
        </div>`,
        link: commonLink
    };
});
myModule.directive("addbtngrp", function () {
    return {
        restrict: "E",
        scope: {
            click: "&",
            disabled: "=",
            cls: "@",
            cdivcls: "@"
        },
        template: `
            <iconbtngrp click="click()" cls="{{cls}}" disabled="disabled" cdivcls="{{cdivcls}}" tooltip="Add">
                <i class="fa-solid fa-plus"></i>
            </iconbtngrp>
        `
    };
});
myModule.directive("editbtngrp", function () {
    return {
        restrict: "E",
        scope: {
            click: "&",
            disabled: "=",
            cls: "@",
            cdivcls: "@"
        },
        template: `
            <iconbtngrp click="click()" cls="{{cls}}" disabled="disabled" cdivcls="{{cdivcls}}" tooltip="Edit">
                <i class="fa-solid fa-pen-to-square"></i>
            </iconbtngrp>
        `
    };
});
myModule.directive("delbtngrp", function () {
    return {
        restrict: "E",
        scope: {
            click: "&",
            disabled: "=",
            cls: "@",
            cdivcls: "@"
        },
        template: `
        <iconbtngrp click="click()" cls="{{cls}}" disabled="disabled" cdivcls="{{cdivcls}}" tooltip="Delete">
            <i class="fa-solid fa-trash btn-imdiate-stack"></i>
            <i class="fa-solid fa-trash btn-imdiate"></i>
        </iconbtngrp>
        `
    };
});
myModule.directive("deatvbtngrp", function () {
    return {
        restrict: "E",
        scope: {
            click: "&",
            disabled: "=",
            cls: "@",
            cdivcls: "@"
        },
        template: `
            <iconbtngrp click="click()" cls="{{cls}}" disabled="disabled" cdivcls="{{cdivcls}}" tooltip="Deactive">
                <i class="fa-solid fa-trash"></i>
            </iconbtngrp>
        `
    };
});
myModule.directive("atvbtngrp", function () {
    return {
        restrict: "E",
        scope: {
            click: "&",
            disabled: "=",
            cls: "@",
            cdivcls: "@"
        },
        template: `
            <iconbtngrp click="click()" cls="{{cls}}" disabled="disabled" cdivcls="{{cdivcls}}" tooltip="Active">
                <i class="fa-solid fa-slash btn-imdiate-stack btn-overlap-clr"></i>
                <i class="fa-solid fa-trash"></i>
            </iconbtngrp>
        `
    };
});
myModule.directive("iconbtngrp", function () {
    return {
        restrict: "E",
        scope: {
            id: "@",
            click: "&",
            disabled: "=",
            cdivcls: "@",
            cls: "@",
            tooltip: "@",
        },
        transclude: true,
        template: `
        <button class="btn-icon" ng-click="click()" ng-disabled="disabled" title="{{tooltip}}">
            <ng-transclude></ng-transclude>
        </button>`,
        link: commonLink
    };
});
function commonLink(scope, element) {
    createDivAndClass(scope, element);
    setSpanWidth(scope, element);
    setRequiredTag(scope);
    setFirstValue(scope);
}
function createDivAndClass(scope, element) {
    if (scope.cdivcls) {
        var wrapper = angular.element('<div></div>');
        wrapper.addClass(scope.cdivcls);
        element.wrapAll(wrapper);
    }

    if (scope.cls) {
        element.addClass(scope.cls);
    }

    if (scope.hasOwnProperty("mt")) {
        var mtVal = scope.mt ? scope.mt : "mt-4";
        angular.forEach(element, function (el) {
            const children = el.children;

            if (children && children.length > 0) {
                angular.forEach(children, function (child) {
                    if (child.tagName && child.tagName.toLowerCase() === 'div') {
                        angular.element(child).addClass(mtVal);
                    }
                });
            } else if (el.tagName && el.tagName.toLowerCase() === 'div') {
                angular.element(el).addClass(mtVal);
            }
        });
    }
}
function setSpanWidth(scope, element) {
    var labelSpan = element.find("span")[0];
    if (labelSpan) {
        function updateOverflow() {
            scope.isOverflow = checkOverflow(labelSpan);
            scope.$applyAsync();
        }
        function checkOverflow(el) {
            return el.scrollWidth > el.offsetWidth;
        }
        setTimeout(updateOverflow, 0);
        window.addEventListener("resize", updateOverflow);
        scope.$on("$destroy", function () {
            window.removeEventListener("resize", updateOverflow);
        });
    }

}
function setRequiredTag(scope) {
    scope.isRequired = angular.isDefined(scope.required);
    if (scope.isRequired) {
        scope.displayLabel = scope.label + " *";
    } else {
        scope.displayLabel = scope.label;
    }
}
function setFirstValue(scope) {
    scope.$evalAsync(function () {
        if (scope.checked && scope.value) { //radio
            scope.model = scope.value;
            if (scope.change) scope.change();
        }
        else if (scope.checked) {//checkbox
            scope.model = scope.checked;
            if (scope.change) scope.change({ newVal: scope.model });
        }
    });
}
function setDisplayPlaceHolder(scope) {
    if (!scope.placeholder) {
        scope.displayPlaceholder = scope.label;
    } else {
        scope.displayPlaceholder = scope.placeholder;
    }
}
function applyTextFilters(scope, element) {
    commonLink(scope, element);
    setDisplayPlaceHolder(scope);

    scope.onEnterKey = function (event) {
        if (event.key === "Enter" && angular.isFunction(scope.onenter)) {
            scope.onenter();
            event.preventDefault();
        }
    };

    scope.filterInput = function (event) {
        if (angular.isDefined(scope.code)) {
            var char = String.fromCharCode(event.which || event.keyCode);
            if (!/^[a-zA-Z0-9\-_#\s]$/.test(char)) {
                event.preventDefault();
            }
            else {
                if (/[a-z]/.test(char)) {
                    event.preventDefault();
                    var upperChar = char.toUpperCase();

                    var input = event.target;
                    var start = input.selectionStart;
                    var end = input.selectionEnd;
                    var value = input.value;

                    input.value = value.substring(0, start) + upperChar + value.substring(end);
                    scope.model = input.value;
                    input.setSelectionRange(start + 1, start + 1); // move cursor
                }
            }
        }
    };

    scope.sameline = angular.isDefined(scope.inline);
    scope.onKeyUp = function (event) {
        if (scope.keyup) {
            scope.keyup({ $event: event });
        }
    };
}
function applyComboFilters(scope, element, timeout) {
    commonLink(scope, element);

    scope.sameline = angular.isDefined(scope.inline);

    const displayFields = scope.display || ['name', 'code'];
    scope.$watch('list', function (list) {
        if (!list) return;
        scope.modelMap = list.map(item => {
            const values = displayFields
                .map(field => item[field])
                .filter(v => v != null && v !== '');
            return { displayName: values.join('-'), original: item };
        });
    });
    
    scope.$watch('modelTemp', function (newVal, oldVal) {
        if (newVal != null && oldVal != null)
            scope.setModel(newVal.original);
    });
}

myModule.directive('fileModel', ['$parse', function ($parse) {
    return {
        restrict: 'A',
        scope: {
            fileModel: '=',
            previewModel: '='
        },
        link: function (scope, element) {
            element.bind('change', function () {
                var file = element[0].files[0];
                scope.fileModel = file;
                if (file && file.type.startsWith("image/")) {
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        scope.$apply(function () {
                            scope.previewModel = e.target.result;
                        });
                    };
                    reader.readAsDataURL(file);
                } else {
                    scope.$apply();
                }
            });
        }
    };
}]);
myModule.directive("pagedTable", function () {
    return {
        restrict: "E",
        scope: {
            headers: "=",
            fetchMethod: "&",
            rowTemplate: "@",
            context: "=",
            onInit: '&?'
        },
        transclude: true,
        template: `
        <div class="row">
            <div class="col-12">
                <table class="table">
                    <thead>
                        <tr>
                            <th ng-repeat="h in headers" ng-style="{'width': h.width}" ng-class="h.class">{{h.title}}</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr ng-repeat="item in pagination.items" ng-include="rowTemplate"></tr>
                    </tbody>
                </table>
            </div>
            <div class="col-12 text-center mt-2">
                <iconbtngrp id="PageFirst" click="pagination.firstPage()" disabled="pagination.isFirstPage" tooltip="First Page">
                    <i class="fa-solid fa-backward-fast"></i>
                </iconbtngrp>
                <iconbtngrp click="pagination.prevPage()" disabled="pagination.isFirstPage" tooltip="Previous Page">
                    <i class="fa-solid fa-arrow-left"></i>
                </iconbtngrp>
                <iconbtngrp ng-repeat="n in pagination.pageNumbers()" click="pagination.listItemsOf(n)" ng-class="{ 'active': pagination.currentPage === n }">
                    {{n}}
                </iconbtngrp>
                <iconbtngrp click="pagination.nextPage()" disabled="pagination.isLastPage" tooltip="Next Page">
                    <i class="fa-solid fa-arrow-right"></i>
                </iconbtngrp>
                <iconbtngrp click="pagination.lastPage()" disabled="pagination.isLastPage" tooltip="Last Page">
                    <i class="fa-solid fa-forward-fast"></i>
                </iconbtngrp>
            </div>
        </div>
        `,
        link: function (scope) {
            scope.pagination = {
                currentPage: 1,
                totalPages: 1,
                totalItemCount: 0,
                pagSize: 10,
                items: [],
                isFirstPage: true,
                isLastPage: true,
                loadPage: function (page) {
                    scope.fetchMethod({
                        pageNumber: page, successFun: (result) => {
                            this.items = result.pagedList;
                            this.isFirstPage = !result.previousPage;
                            this.isLastPage = !result.nextPage;
                            this.totalPages = result.totalPages;
                            this.totalItemCount = result.totalCount;
                            this.currentPage = result.pageIndex;
                        }
                    });
                },
                firstPage() {
                    this.loadPage(1);
                },
                prevPage() {
                    if (this.currentPage > 1)
                        this.loadPage(this.currentPage - 1);
                },
                nextPage() {
                    if (this.currentPage < this.totalPages)
                        this.loadPage(this.currentPage + 1);
                },
                lastPage() {
                    this.loadPage(this.totalPages);
                },

                listItemsOf(pageNo) {
                    this.loadPage(pageNo);
                },
                refreshCurrentPage() {
                    scope.pagination.loadPage(scope.pagination.currentPage);
                },
                pageNumbers() {
                    var input = []; var startPage = 1; var endPage = this.totalPages;

                    if (this.currentPage - 2 > 0)
                        startPage = this.currentPage - 1;
                    if (this.currentPage + 1 < this.totalPages)
                        endPage = this.currentPage + 1;

                    if (endPage == 2 && this.totalPages > 3)
                        ++endPage;
                    if ((endPage - startPage) == 1 && this.totalPages > 3)
                        --startPage;

                    for (var i = startPage; i <= endPage; i += 1) {
                        input.push(i);
                    }
                    return input;
                }
            };
            if (scope.onInit) {
                scope.onInit({ api: { refresh: scope.pagination.refreshCurrentPage } });
            }

            scope.pagination.firstPage();
        }
    };
});
myModule.directive("inputSelector", function () {
    return {
        restrict: "E",
        scope: {
            mt:'@',
            label: '@',
            cdivcls: "@",
            filterby: '=',
            inputsearch: '=?',
            list: '=',
            onSelect: '&',
            model: "=",
            fetchMethod: "&",
        },
        transclude: true,
        template: `
        <div class="row">
            <div class="col-12">
                <div class="input-group">
                    <div class="input-group-prepend">
                        <span class="input-group-text" ng-class="{\'rem6dot5\': !isOverflow}">{{label}}</span>
                    </div>
                    <input id="posItemName" placeholder="{{label}}" class="input-group-prepend form-control"
                           type="text" maxlength="100" ng-model="displayName"
                           style="flex: 1 1 auto;" ng-disabl ng-disabled="true"></input>
                    <div class="input-group-postpend">
                        <button title="Search" style="height: 100%;" ng-click="commonSelector.open()">
                            <i class="fa-solid fa-magnifying-glass"></i>
                        </button>
                    </div>
                </div>
                <div ng-transclude></div>
            </div>
        </div>
        <common-selector id="inpSelector" api="commonSelector" label="{{label}}" list="list" filterby="filterby" model="model">
        </common-selector>
        `,
        link: function (scope, element) {
            applyTextFilters(scope, element);
            scope.$watch('model', function (item) {
                if (!item) return;
                const searchFields = scope.filterby || ['name', 'code'];
                const values = searchFields
                    .map(field => item[field])
                    .filter(v => v != null && v !== '');
                scope.displayName = values.join(', ');
            }, true);

        }
    };
});
myModule.directive("commonSelector", function () {
    return {
        restrict: "E",
        scope: {
            id: "@",
            model: "=",
            label: '@',
            filterby: '=',
            inputsearch: '=?',
            list: '=',
            fetchMethod: "&",
            api: '=?',
        },
        transclude: true,
        template: `
        <div class="modal fade" id="cs_{{id}}" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        {{label}} Selector
                        <button type="button" class="close" ng-click="closePopup()" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-12">
                                <txtgrp id="popupSearch" label="{{label}}" maxlength="100"
                                        model="inputsearch" mt="mt-0" keyup="pagination.firstPage()"></txtgrp>
                                <span ng-transclude class="mt-0"></span>
                            </div>
                            <div class="col-12">
                                <table class="table mt-4">
                                    <thead>
                                        <tr>
                                            <th width="80%">{{label}}</th>
                                            <th></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr ng-repeat="item in pagination.items">
                                            <td>{{item.displayName}}</td>
                                            <td align="right">
                                                <iconbtngrp id="selectItemBtn" click="selectItem(item)">
                                                    <i class="fa-solid fa-check"></i>
                                                </iconbtngrp>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            <div class="col-12 text-center mt-2">
                                <iconbtngrp id="PageFirst" click="pagination.firstPage()" disabled="pagination.isFirstPage" tooltip="First Page">
                                    <i class="fa-solid fa-backward-fast"></i>
                                </iconbtngrp>
                                <iconbtngrp click="pagination.prevPage()" disabled="pagination.isFirstPage" tooltip="Previous Page">
                                    <i class="fa-solid fa-arrow-left"></i>
                                </iconbtngrp>
                                <iconbtngrp ng-repeat="n in pagination.pageNumbers()" click="pagination.listItemsOf(n)" ng-class="{ 'active': pagination.currentPage === n }">
                                    {{n}}
                                </iconbtngrp>
                                <iconbtngrp click="pagination.nextPage()" disabled="pagination.isLastPage" tooltip="Next Page">
                                    <i class="fa-solid fa-arrow-right"></i>
                                </iconbtngrp>
                                <iconbtngrp click="pagination.lastPage()" disabled="pagination.isLastPage" tooltip="Last Page">
                                    <i class="fa-solid fa-forward-fast"></i>
                                </iconbtngrp>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>`,
        link: function (scope) {
            scope.pagination = {
                currentPage: 1,
                totalPages: 1,
                totalItemCount: 0,
                pageSize: 10,
                items: [],
                isFirstPage: true,
                isLastPage: true,
                loadPage: function (page) {
                    const s = (scope.inputsearch || '').toLowerCase();
                    const searchFields = scope.filterby || ['name', 'code'];
                    if (scope.list) {
                        const mapped = scope.list.filter(item => {
                            return searchFields.some(field => {
                                const value = (item[field] || '').toString().toLowerCase();
                                return value.includes(s);
                            });
                        }).map(item => {
                            const values = searchFields
                                .map(field => item[field])
                                .filter(v => v != null && v !== '');
                            return { displayName: values.join(', '), original: item };
                        });

                        this.totalItemCount = mapped.length;
                        this.totalPages = Math.ceil(this.totalItemCount / this.pageSize);
                        this.currentPage = Math.min(Math.max(page, 1), this.totalPages || 1);
                        var startIndex = (this.currentPage - 1) * this.pageSize;
                        var endIndex = startIndex + this.pageSize;
                        this.items = mapped.slice(startIndex, endIndex);
                        this.isFirstPage = page === 1;
                        this.isLastPage = page === this.totalPages;
                    } else {
                        scope.fetchMethod({
                            pageNumber: page,
                            pageSize: 10,
                            searchBy: s,
                            successFun: (result) => {
                                this.items = result.pagedList.map(item => {
                                    const values = searchFields
                                        .map(field => item[field])
                                        .filter(v => v != null && v !== '');
                                    return { displayName: values.join(', '), original: item };
                                });
                                this.isFirstPage = !result.previousPage;
                                this.isLastPage = !result.nextPage;
                                this.totalPages = result.totalPages;
                                this.totalItemCount = result.totalCount;
                                this.currentPage = result.pageIndex;
                            }
                        });
                    }

                },
                firstPage() {
                    this.loadPage(1);
                },
                prevPage() {
                    if (this.currentPage > 1)
                        this.loadPage(this.currentPage - 1);
                },
                nextPage() {
                    if (this.currentPage < this.totalPages)
                        this.loadPage(this.currentPage + 1);
                },
                lastPage() {
                    this.loadPage(this.totalPages);
                },

                listItemsOf(pageNo) {
                    this.loadPage(pageNo);
                },
                refreshCurrentPage() {
                    scope.pagination.loadPage(scope.pagination.currentPage);
                },
                pageNumbers() {
                    var input = []; var startPage = 1; var endPage = this.totalPages;

                    if (this.currentPage - 2 > 0)
                        startPage = this.currentPage - 1;
                    if (this.currentPage + 1 < this.totalPages)
                        endPage = this.currentPage + 1;

                    if (endPage == 2 && this.totalPages > 3)
                        ++endPage;
                    if ((endPage - startPage) == 1 && this.totalPages > 3)
                        --startPage;

                    for (var i = startPage; i <= endPage; i += 1) {
                        input.push(i);
                    }
                    return input;
                }
            };
            scope.selectedItem = {};
            scope.showPoupUp = function () {
                scope.pagination.firstPage();
                $('#cs_' + scope.id).modal('show');
            }
            scope.closePopup = function (event) {
                $('#cs_' + scope.id).modal('hide');
            };
            scope.selectItem = function (item) {
                scope.model = item.original;
                scope.closePopup();
            };
            scope.api = {
                open: scope.showPoupUp,
                close: scope.closePopup
            };
            scope.$on('popupActivity:closed', function () {
                scope.pagination.lastPage();
            });
        }
    };
});
myModule.directive("popupActivity", function ($controller, $compile, $http, $templateCache) {
    return {
        restrict: "E",
        scope: {
            templateurl: "@",
            controller: "@",
            label: '@',
            api: '=?',
            onLoaded: "&"
        },
        transclude: true,
        template: `
        <div class="modal fade" id="PopupActivity" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        {{label}}
                        <button type="button" class="close" ng-click="closePopup()" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body" id="popupContent">
                    </div>
                </div>
            </div>
        </div>`,
        link: function (scope, element) {
            let ctrl;
            if (scope.controller) {
                ctrl = $controller(scope.controller, { $scope: scope });
            }

            $http.get(scope.templateurl, { cache: $templateCache })
                .then(function (response) {
                    const content = $compile(response.data)(scope);
                    element.find("#popupContent").html(content);

                    scope.$on('mobileTagGrp:ready', function () { scope.addClicked() });
                });

            scope.showPoupUp = function () {
                if (scope.controller == "CustomerController") scope.addClicked();
                $('#PopupActivity').modal('show');
            }
            scope.closePopup = function () {
                scope.$emit('popupActivity:closed');
                $('#PopupActivity').modal('hide');
            };
            scope.api = {
                open: scope.showPoupUp,
                close: scope.closePopup
            };
        }
    };
});
myModule.directive("itemGridSearch", function () {
    return {
        restrict: "E",
        scope: {
            itemService: "=",
            selected: "&",
            itemSelected:"="
        },
        controller: function ($scope) {
            $scope.currency = GetValueFromCache("curSymbole");
            $scope.defaultImage = 'data:image/jpeg;base64,' + GetValueFromCache("defaultImg");
            $scope.input = { category: '', code: '', name: '' };
            $scope.categories = [];
            $scope.autoSearch = false;
            $scope.refreshItem = refreshItem;
            $scope.categoryChanged = function () {
                refreshItem();
            }
            loadCategories();
            function loadCategories() {
                $scope.itemService.getAllLeafCategories(function (result) {
                    $scope.categories = result;
                });
            };
            function refreshItem() {
                $scope.pagination.firstPage();
            };
            $scope.searchChanged = function (event) {
                if (event.key != "Enter" && !$scope.autoSearch)
                    return;

                refreshItem();
            };


            $scope.codeInput = function (event) {
                var char = String.fromCharCode(event.which || event.keyCode);
                if (!/^[a-zA-Z0-9\-_#\s]$/.test(char)) {
                    event.preventDefault();
                }
                else {
                    if (/[a-z]/.test(char)) {
                        event.preventDefault();
                        var upperChar = char.toUpperCase();

                        var input = event.target;
                        var start = input.selectionStart;
                        var end = input.selectionEnd;
                        var value = input.value;

                        input.value = value.substring(0, start) + upperChar + value.substring(end);
                        $scope.model = input.value;
                        input.setSelectionRange(start + 1, start + 1); // move cursor
                    }
                }
                console.log(event.target.value);

            };
        },
        transclude: true,
        template: `
        <div class="row">
            <cmbgrp id="posItemCat" label="Category" cdivcls="col-md-4" mt="mt-0" changed="categoryChanged()"
                    model="input.category" list="categories" display="['name','path']" required></cmbgrp>
            <div class="col-md-8">
                <div class="input-group">
                    <div class="input-group-prepend">
                        <span class="input-group-text" ng-class="{\'rem6dot5\': !isOverflow}">Item</span>
                    </div>
                    <input id="posItemCode" placeholder="Code ↲" class="form-control"
                           type="text" maxlength="20" ng-model="input.code"
                           ng-keyup="searchChanged($event)"
                            ng-keypress="codeInput($event)"
                           style="flex: 1 0 100px; max-width: 120px;"></input>
                    <input id="posItemName" placeholder="Name ↲" class="input-group-prepend form-control"
                           type="text" maxlength="100" ng-model="input.name"
                           ng-keyup="searchChanged($event)"
                           style="flex: 1 1 auto;"></input>
                    <div class="input-group-postpend">
                        <button title="Current Page/Total Pages" style="height: 100%;" ng-click="refreshItem()">
                            <i class="fa-solid fa-magnifying-glass"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-2 mt-4" ng-repeat="item in pagination.items">
                <button type="button" class="btn-cart" ng-click="itemSelected(item)">
                    <div class="fixed-img-div">
                      <img ng-src="{{item.imagePreview || defaultImage}}" alt="Image" style="width:100%; height:100%;">
                    </div>
                    {{currency}} {{item.price}} <span ng-if="item.allowSPriceChange" class="tag">Ed</span>
                    <br/><b>{{item.code}}</b>
                    <br/>{{item.name}}
                </button>
            </div>
        </div>
        <div class="row">
            <div class="col-12 text-center mt-2">
                <span title="Items Dispalying">[{{((pagination.currentPage-1) * pagination.pagSize) + 1}}-{{((pagination.currentPage-1) * pagination.pagSize) + pagination.items.length}}]/{{pagination.totalItemCount}}</span>
                <iconbtngrp id="PageFirst" click="pagination.firstPage()" disabled="pagination.isFirstPage" tooltip="First Page">
                    <i class="fa-solid fa-backward-fast"></i>
                </iconbtngrp>
                <iconbtngrp click="pagination.prevPage()" disabled="pagination.isFirstPage" tooltip="Previous Page">
                    <i class="fa-solid fa-arrow-left"></i>
                </iconbtngrp>
                <iconbtngrp ng-repeat="n in pagination.pageNumbers()" click="pagination.listItemsOf(n)" ng-class="{ 'active': pagination.currentPage === n }">
                    {{n}}
                </iconbtngrp>
                <iconbtngrp click="pagination.nextPage()" disabled="pagination.isLastPage" tooltip="Next Page">
                    <i class="fa-solid fa-arrow-right"></i>
                </iconbtngrp>
                <iconbtngrp click="pagination.lastPage()" disabled="pagination.isLastPage" tooltip="Last Page">
                    <i class="fa-solid fa-forward-fast"></i>
                </iconbtngrp>
                <span title="Current Page/Total Pages">{{pagination.currentPage}}/{{pagination.totalPages}} </span>
            </div>
        </div>
        
        `,
        link: function (scope) {
            scope.itemSelect = function (item) {
                scope.itemSelected(item);
            };
            scope.pagination = {
                currentPage: 1,
                totalPages: 1,
                totalItemCount: 0,
                pagSize: 24,
                items: [],
                isFirstPage: true,
                isLastPage: true,
                loadPage: function (page) {
                    console.log(scope.input.code);

                    scope.itemService.getItem(page, scope.pagination.pagSize, scope.input.category.code, scope.input.code, scope.input.name, function (result) {
                        result.pagedList.forEach(function (item) {
                            if (item.image != null && item.image != '')
                                item.imagePreview = 'data:image/jpeg;base64,' + item.image;
                        });
                        scope.pagination.items = result.pagedList;
                        scope.pagination.isFirstPage = !result.previousPage;
                        scope.pagination.isLastPage = !result.nextPage;
                        scope.pagination.totalPages = result.totalPages;
                        scope.pagination.totalItemCount = result.totalCount;
                        scope.pagination.currentPage = result.pageIndex;
                    });
                },
                firstPage() {
                    this.loadPage(1);
                },
                prevPage() {
                    if (this.currentPage > 1)
                        this.loadPage(this.currentPage - 1);
                },
                nextPage() {
                    if (this.currentPage < this.totalPages)
                        this.loadPage(this.currentPage + 1);
                },
                lastPage() {
                    this.loadPage(this.totalPages);
                },
                listItemsOf(pageNo) {
                    this.loadPage(pageNo);
                },
                refreshCurrentPage() {
                    scope.pagination.loadPage(scope.pagination.currentPage);
                },
                pageNumbers() {
                    var input = []; var startPage = 1; var endPage = this.totalPages;

                    if (this.currentPage - 2 > 0)
                        startPage = this.currentPage - 1;
                    if (this.currentPage + 1 < this.totalPages)
                        endPage = this.currentPage + 1;

                    if (endPage == 2 && this.totalPages > 3)
                        ++endPage;
                    if ((endPage - startPage) == 1 && this.totalPages > 3)
                        --startPage;

                    for (var i = startPage; i <= endPage; i += 1) {
                        input.push(i);
                    }
                    return input;
                }
            };
            if (scope.onInit) {
                scope.onInit({ api: { refresh: scope.pagination.refreshCurrentPage } });
            }
        }
    };
});
myModule.directive("qtygrp", function () {
    return {
        restrict: "E",
        scope: {
            model: "=",
            removeItem: "&",
            qtyChanged: "&"
        },
        transclude: true,
        template: `
        <button ng-click="decreaseModel()" tooltip="Decrease">-</button>
            <span tooltip="Increase">{{model}}</span>
        <button ng-click="increaseModel()" tooltip="Increase">+</button>`,
        controller: function ($scope, $timeout) {
            $scope.increaseModel = function () { $scope.model = $scope.model + 1; $timeout(() => $scope.qtyChanged(), 0); }
            $scope.decreaseModel = function () {
                if ($scope.model > 1) {
                    $scope.model = $scope.model - 1; $timeout(() => $scope.qtyChanged(), 0);
                }
                else if (confirm("Do you want to remove?")) {
                    $scope.removeItem();
                }
            }
        }
    };
});
myModule.directive("editgrp", function ($timeout) {
    return {
        restrict: "E",
        scope: {
            id: "@",
            model: "=",
            description: "@",
            must: "&",
            edited: "&" // expression binding
        },
        template: `
            <a href ng-click="openEdit()" class="text-decoration-none">{{model}}</a>

            <div class="modal fade" tabindex="-1" role="dialog">
                <div class="modal-dialog modal-dialog-centered" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Edit {{description}}</h5>
                            <button type="button" class="btn-close" ng-click="closeEdit()" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <input id="{{id}}editInp" type="text" ng-model="tempValue" class="form-control" ng-keypress="filterInput($event)" />
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-primary" ng-click="saveEdit()">Save</button>
                            <button type="button" class="btn btn-secondary" ng-click="closeEdit()">Cancel</button>
                        </div>
                    </div>
                </div>
            </div>
        `,
        link: function (scope, element, $attrs) {
            const modal = element.find(".modal");
            const isNumberOnly = $attrs.hasOwnProperty('number');
            scope.filterInput = function (event) {
                if (isNumberOnly) {
                    const char = String.fromCharCode(event.which || event.keyCode);
                    if (event.ctrlKey || event.metaKey || char === '') return;

                    const input = event.target;
                    const currentValue = input.value;
                    const newValue = currentValue + char;

                    if (!/^\d*\.?\d*$/.test(newValue)) {
                        event.preventDefault();
                    }
                }
            };

            const bsModal = new bootstrap.Modal(modal[0], {
                backdrop: "static",
                keyboard: false
            });

            scope.openEdit = function () {
                scope.tempValue = scope.model;
                bsModal.show();
            };

            scope.closeEdit = function () {
                bsModal.hide();
            };

            scope.saveEdit = function () {
                var valid = true;

                RemoveControlError(scope.id + "editInp");
                if ($attrs.must) {
                    var error = scope.must({ tempValue: scope.tempValue });
                    if (error != "") {
                        SetControlError(scope.id +"editInp", error); valid = false;
                    }
                }

                if (!valid)
                    return;
                if (isNumberOnly)
                    scope.model = Number(parseFloat(scope.tempValue).toFixed(2));
                else
                    scope.model = scope.tempValue;
                $timeout(() => {
                    bsModal.hide();
                    if (scope.edited) scope.edited();
                }, 0);
                
            };
        }
    };
});
