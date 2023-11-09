$(document).ready(function () {
    var datas = [
        {
            el: $('.cbNhaCC'),
            placeholder: '-- Nhà cung cấp --',
            url: '/QuanLy/NhapKho/api/nccs'
        },
        {
            el: $('.cbHangHoa'),
            placeholder: '-- Hàng hoá --',
            url: '/QuanLy/NhapKho/api/hhs'
        },
        {
            el: $('.cbNhomHangHoa'),
            placeholder: '-- Nhóm hàng hoá --',
            url: '/DanhMuc/HangHoa/api/nhhs'
        }
    ];
    configCb(datas);
    configDate();
});
function loadTableBaoCaoTongHop() {
    showLoader($('#tableTongHop'));
    $.ajax({
        type: "post",
        url: "/QuanLy/BaoCaoTon/search-table-baocaotonghop",
        data: "idNhh=" + $('#NhhTH').val() + "&idHh=" + $('#hangHoaTH').val()
            + "&fromDay=" + $('#fromDayTH').val() + "&toDay=" + $('#toDayTH').val(),
        success: function (result) {
            hideLoader();
            $('#tBodyBaoCaoTongHop').replaceWith(result);
        },
        error: function (loi) {
            console.log(loi);
        }
    });
}
function loadTableBaoCaoChiTiet() {
    showLoader($('#tableChiTiet'));
    $.ajax({
        type: "post",
        url: "/QuanLy/BaoCaoTon/search-table-baocaochitiet",
        data: "idNhh=" + $('#NhhCT').val() + "&idHh=" + $('#hangHoaCT').val()
            + "&fromDay=" + $('#fromDayCT').val() + "&toDay=" + $('#toDayCT').val() + "&idNcc=" + $('#nhaCCCT').val(),
        success: function (result) {
            hideLoader();
            $('#tBodyBaoCaoChiTiet').replaceWith(result);
        },
        error: function (loi) {
            console.log(loi);
        }
    });
}
function downloadBaoCaoTongHopPDF() {
    spinnerBtn($('#btnBaoCaoTongHopPDF'));
    $.ajax({
        type: "post",
        url: "/QuanLy/BaoCaoTon/download/BaoCaoTongHopPDF",
        data: "idNhh=" + $('#NhhTH').val() + "&idHh=" + $('#hangHoaTH').val()
            + "&fromDay=" + $('#fromDayTH').val() + "&toDay=" + $('#toDayTH').val(),
        xhrFields: {
            responseType: 'blob'
        },
        success: function (result) {
            showBtn($('#btnBaoCaoTongHopPDF'), `<i class="lni lni-printer"></i>Xuất PDF`);
            var a = document.createElement('a');
            var url = window.URL.createObjectURL(result);
            a.href = url;
            a.download = "BaoCaoTongHopPDF.pdf";
            document.body.appendChild(a);
            a.click();
            setTimeout(function () {
                document.body.removeChild(a);
                window.URL.revokeObjectURL(url);
            }, 0);
        },
        error: function (loi) {
            console.log(loi);
        }
    });
}
function downloadBaoCaoTongHopExcel() {
    spinnerBtn($('#btnBaoCaoTongHopExcel'));
    $.ajax({
        type: "post",
        url: "/QuanLy/BaoCaoTon/download/BaoCaoTongHopExcel",
        data: "idNhh=" + $('#NhhTH').val() + "&idHh=" + $('#hangHoaTH').val()
            + "&fromDay=" + $('#fromDayTH').val() + "&toDay=" + $('#toDayTH').val(),
        xhrFields: {
            responseType: 'blob'
        },
        success: function (result) {
            showBtn($('#btnBaoCaoTongHopExcel'), `<i class="lni lni-save"></i>Xuất Excel</button>`);
            var a = document.createElement('a');
            var url = window.URL.createObjectURL(result);
            a.href = url;
            a.download = "BaoCaoTongHopExcel.xlsx";
            document.body.appendChild(a);
            a.click();
            setTimeout(function () {
                document.body.removeChild(a);
                window.URL.revokeObjectURL(url);
            }, 0);
        },
        error: function (loi) {
            console.log(loi);
        }
    });
}
function downloadBaoCaoChiTietPDF() {
    spinnerBtn($('#btnBaoCaoChiTietPDF'));
    $.ajax({
        type: "post",
        url: "/QuanLy/BaoCaoTon/download/BaoCaoChiTietPDF",
        data: "idNhh=" + $('#NhhCT').val() + "&idHh=" + $('#hangHoaCT').val()
            + "&fromDay=" + $('#fromDayCT').val() + "&toDay=" + $('#toDayCT').val() + "&idNcc=" + $('#nhaCCCT').val(),
        xhrFields: {
            responseType: 'blob'
        },
        success: function (result) {
            showBtn($('#btnBaoCaoChiTietPDF'), `<i class="lni lni-printer"></i>Xuất PDF`);
            var a = document.createElement('a');
            var url = window.URL.createObjectURL(result);
            a.href = url;
            a.download = "BaoCaoChiTietPDF.pdf";
            document.body.appendChild(a);
            a.click();
            setTimeout(function () {
                document.body.removeChild(a);
                window.URL.revokeObjectURL(url);
            }, 0);

        },
        error: function (loi) {
            console.log(loi);
        }
    });
}
function downloadBaoCaoChiTietExcel() {
    spinnerBtn($('#btnBaoCaoChiTietExcel'));
    $.ajax({
        type: "post",
        url: "/QuanLy/BaoCaoTon/download/BaoCaoChiTietExcel",
        data: "idNhh=" + $('#NhhCT').val() + "&idHh=" + $('#hangHoaCT').val()
            + "&fromDay=" + $('#fromDayCT').val() + "&toDay=" + $('#toDayCT').val() + "&idNcc=" + $('#nhaCCCT').val(),
        xhrFields: {
            responseType: 'blob'
        },
        success: function (result) {
            showBtn($('#btnBaoCaoChiTietExcel'), `<i class="lni lni-save"></i>Xuất Excel</button>`);
            var a = document.createElement('a');
            var url = window.URL.createObjectURL(result);
            a.href = url;
            a.download = "BaoCaoChiTietExcel.xlsx";
            document.body.appendChild(a);
            a.click();
            setTimeout(function () {
                document.body.removeChild(a);
                window.URL.revokeObjectURL(url);
            }, 0);
            $('#btnBaoCaoChiTietExcel').show();
            $('#spinner').remove();
        },
        error: function (loi) {
            console.log(loi);
        }
    });
}