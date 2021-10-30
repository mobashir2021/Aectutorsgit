function Pager(searchCriteria, maxPageIndex, pageCallBack) {
    var self = this;
    self.MaxPageIndex = ko.observable(maxPageIndex);
    self.PageSize = ko.observable(10);
    self.PageIndex = ko.observable(0);
    self.SearchCriteria = searchCriteria;
    self.PageCallBack = pageCallBack;

    self.previousPage = function () {
        if (self.PageIndex() > 0) {
            self.moveToPage(self.PageIndex() - 1);
        }
    };
    self.nextPage = function () {
        if (self.PageIndex() < self.MaxPageIndex()) {
            self.moveToPage(self.PageIndex() + 1);
        }
    };
    self.allPages = ko.dependentObservable(function () {
        var pages = [];
        for (i = 0; i <= self.MaxPageIndex(); i++) {
            pages.push({ pageNumber: (i + 1) });
        }
        return pages;
    });
    self.moveToPage = function (index) {
        self.PageIndex(index);
        searchCriteria.PageIndex = index;
        self.PageCallBack();
    };
}