(function (window, document, $, undefined) {
    var grid = $('#grid');
    var rowsPerPage = $('#rowsPerPage');
    var filter = $('#filter');
    var self = this;

    $('#clearText').click(function () {
        filter.val('');
        state.filter = '';
        state.page = 1;
        _updateGrid();
        return false;
    });

    $('#resetFilter').click(function () {
        filter.val('');
        rowsPerPage.val(10);

        state = state = {
            sort: '',
            sortDir: 0,
            page: 1,
            filter: '',
            perPage: 10
        };

        _updateGrid();
        return false;
    });

    rowsPerPage.bind("change", function (e) {
        _delay(function () {
            state.page = 1;
            state.perPage = rowsPerPage.val();
            _updateGrid();
        }, 500);
    });

    filter.bind("keyup", function (e) {
        if (e.keyCode == 16 ||
            e.keyCode == 17 ||
            e.keyCode == 18 ||
            e.keyCode == 32) return;
        if (e.keyCode == 27) {
            filter.val('');
            return;
        }
        if (e.keyCode == 13) {
            state.page = 1;
            _updateGrid();
            return;
        }

        _delay(function () {
            state.filter = filter.val();
            state.page = 1;
            _updateGrid();
        }, 500);
    });

    var state = {
        sort: '',
        sortDir: 0,
        page: 1,
        filter: '',
        perPage: 10
    };

    grid.on('click', 'li', function (e) {
        state.page = $(e.target).data('page');
        _updateGrid();
    });

    grid.on('click', 'th a', function (e) {
        e.preventDefault();
        var url = $(e.target).attr('href');

        if (url == undefined || url == '') {
            return;
        }

        var parsed = _parseQueryString(url.substring(url.indexOf('?') + 1));

        if (state.sort == parsed.sort) {
            state.sortDir = state.sortDir == 0 ? 1 : 0;
        } else {
            state.sort = parsed.sort;
            state.sortDir = 0;
        }

        _updateGrid();
    });

    var _delay = (function () {
        var timer = 0;
        return function (callback, ms) {
            clearTimeout(timer);
            timer = setTimeout(callback, ms);
        };
    })();

    var _parseQueryString = function (queryString) {
        var params = {}, queries, temp, i, l;
        queries = queryString.split("&");
        for (i = 0, l = queries.length; i < l; i++) {
            temp = queries[i].split('=');
            params[temp[0]] = temp[1];
        }
        return params;
    };

    var _updateGrid = function () {
        if (this.options.useState) {
            localStorage[this.options.storageKey] = JSON.stringify(state);
        }

        $.ajax({
            type: 'POST',
            url: updateUrl,
            data: {
                page: state.page,
                sort: state.sort,
                sortDir: state.sortDir,
                filter: state.filter,
                perPage: state.perPage,
                data: JSON.stringify(self.options.data)
            }
        }).done(function (data) {
            grid.html(data);
        });
    };

    var init = function (updateUrl, opt) {
        opt = opt || {};
        self.options = {};
        self.updateUrl = updateUrl;
        self.options.storageKey = opt.storageKey || window.location.pathname;
        self.options.useState = opt.useState || true;
        self.options.data = opt.data || '';

        if (self.options.useState) {
            var jsonState = localStorage[this.options.storageKey];

            if (jsonState != null && jsonState != undefined) {
                state = JSON.parse(jsonState);
            }
        }

        rowsPerPage.val(state.perPage);
        filter.val(state.filter);

        _updateGrid();
    };

    window.initWebGrid = init;
}(window, document, jQuery));