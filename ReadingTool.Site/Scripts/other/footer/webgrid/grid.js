//TODO fixme
function newState() {
    return {
        sort: 'language',
        sortDir: 0,
        page: 1,
        filter: '',
        perPage: 15
    };
}

function reset(everything) {
    $('#filter').val('');

    if (everything) {
        state = newState();
    }

    updateGrid();
    return false;
}

function updateGrid() {
    localStorage[storageKey] = JSON.stringify(state);

    $.post(
        gridPostUrl, {
            page: state.page,
            sort: state.sort,
            sortDir: state.sortDir,
            filter: state.filter,
            perPage: state.perPage
        },
        function (data) {
            $('#grid').html(data);
            $('#grid table thead tr th:first').html('<input type="checkbox" id="checkAll" />');
        }
    );
}

$('#rowsPerPage').bind("change", function(e) {
    delay(function () {
        state.page = 1;
        state.perPage = $('#rowsPerPage').val();
        updateGrid();
    }, 500);
});

$("#filter").bind("keyup", function (e) {
    if (
        e.keyCode == 16 ||
            e.keyCode == 17 ||
            e.keyCode == 18 ||
            e.keyCode == 32
    ) return;
    if (e.keyCode == 27) {
        $('#filter').val('');
        return;
    }
    if (e.keyCode == 13) {
        state.page = 1;
        updateGrid();
        return;
    }

    delay(function () {
        state.filter = $('#filter').val();
        state.page = 1;
        updateGrid();
    }, 500);
});

function updateCheckboxes() {
    var state = $('#checkAll').attr('checked');
    if (state == undefined) state = false;
    $('.table input[type=checkbox]').each(function() {
        $(this).attr('checked', state);
    });
}

$('#grid').on('click', 'li', function (e) {
    state.page = $(e.target).data('page');
    updateGrid();
});

$('#grid').on('click', 'thead ', function (e) {
    if ($(e.target).is('input')) {
        updateCheckboxes();
        return;
    }
    
    console.info(e.target);
    e.preventDefault();
    return;
    
    e.preventDefault();
    var url = $(e.target).attr('href');
    
    if(url==undefined || url=='') {
        return;
    }
    
    var parsed = parseQueryString(url.substring(url.indexOf('?') + 1));

    if (state.sort == parsed.sort) {
        state.sortDir = state.sortDir == 0 ? 1 : 0;
    } else {
        state.sort = parsed.sort;
        state.sortDir = 0;
    }

    updateGrid();
});

var parseQueryString = function (queryString) {
    var params = {}, queries, temp, i, l;
    queries = queryString.split("&");
    for (i = 0, l = queries.length; i < l; i++) {
        temp = queries[i].split('=');
        params[temp[0]] = temp[1];
    }
    return params;
};

var delay = (function () {
    var timer = 0;
    return function (callback, ms) {
        clearTimeout(timer);
        timer = setTimeout(callback, ms);
    };
})();