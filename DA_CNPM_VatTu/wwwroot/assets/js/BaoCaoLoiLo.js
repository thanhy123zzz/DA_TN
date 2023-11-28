var _chartGiaTriNhapXuat = null;
var _chartTop10NhomHangBanChay = null;
var _chartTop5HangHoaLoiNhuan = null;
$(document).ready(function () {
    // Biêu đồ giá trị nhập xuất
    $('#ngayGiaTriNhapXuat').on('change', function () {
        $.ajax({
            type: "post",
            url: "/QuanLy/BaoCaoLoiLo/getGiaTriNhapXuat",
            data: "tg=" + $('#ngayGiaTriNhapXuat').val(),
            success: function (result) {
                updateTableGiaTriNhapXuat(result);
                const ngayValues = [];
                const tongGtPns = [];
                const tongGtPxs = [];
                for (const item of result) {
                    ngayValues.push(item.ngay);
                    tongGtPns.push(item.tongGtPn);
                    tongGtPxs.push(item.tongGtPx);
                }
                var data = {
                    labels: ngayValues,
                    datasets: [
                        {
                            label: "Tổng nhập",
                            backgroundColor: "transparent",
                            borderColor: "#4a6cf7",
                            data: tongGtPns,
                            pointBackgroundColor: "transparent",
                            pointHoverBackgroundColor: "#4a6cf7",
                            pointBorderColor: "transparent",
                            pointHoverBorderColor: "#fff",
                            pointHoverBorderWidth: 3,
                            pointBorderWidth: 5,
                            pointRadius: 5,
                            pointHoverRadius: 8,
                        },
                        {
                            label: "Tổng xuất",
                            backgroundColor: "transparent",
                            borderColor: "#9b51e0",
                            data: tongGtPxs,
                            pointBackgroundColor: "transparent",
                            pointHoverBackgroundColor: "#9b51e0",
                            pointBorderColor: "transparent",
                            pointHoverBorderColor: "#fff",
                            pointHoverBorderWidth: 3,
                            pointBorderWidth: 5,
                            pointRadius: 5,
                            pointHoverRadius: 8,
                        },
                    ],
                };

                // Cấu hình biểu đồ
                var options = {
                    responsive: true,
                    maintainAspectRatio: false,
                    tooltips: {
                        intersect: false,
                        backgroundColor: "#fbfbfb",
                        titleFontColor: "#8F92A1",
                        titleFontSize: 16,
                        titleFontFamily: "Inter",
                        titleFontStyle: "400",
                        bodyFontFamily: "Inter",
                        bodyFontColor: "#171717",
                        bodyFontSize: 16,
                        multiKeyBackground: "transparent",
                        displayColors: false,
                        xPadding: 30,
                        yPadding: 15,
                        borderColor: "rgba(143, 146, 161, .1)",
                        borderWidth: 1,
                        title: false,
                        callbacks: {
                            label: function (tooltipItem, data) {
                                var datasetLabel = data.datasets[tooltipItem.datasetIndex].label || '';
                                var value = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
                                return datasetLabel + ': ' + formatOddNumber(value);
                            }
                        }
                    },
                    title: {
                        display: false,
                    },

                    layout: {
                        padding: {
                            top: 0,
                        },
                    },
                    legend: false,
                    scales: {
                        yAxes: [
                            {
                                ticks: {
                                    callback: function (value, index, values) {
                                        if (value > 1000000000) {
                                            return value / 1000000000 + " tỉ VNĐ";
                                        }
                                        if (value > 1000000) {
                                            return value / 1000000 + " triệu VNĐ";
                                        }
                                        if (value > 1000) {
                                            return value / 1000 + " ngàn VNĐ";
                                        }
                                        return value;
                                    }
                                },
                                gridLines: {
                                    display: false,
                                    drawTicks: false,
                                    drawBorder: false,
                                }
                            },
                        ],
                        xAxes: [
                            {
                                gridLines: {
                                    drawBorder: false,
                                    color: "rgba(143, 146, 161, .1)",
                                    zeroLineColor: "rgba(143, 146, 161, .1)",
                                },
                                ticks: {
                                    padding: 20,
                                },
                            },
                        ],
                    }
                };

                // Kiểm tra xem đối tượng Chart đã được khởi tạo hay chưa
                if (_chartGiaTriNhapXuat) {
                    _chartGiaTriNhapXuat.data.labels = ngayValues;
                    _chartGiaTriNhapXuat.data.datasets[0].data = tongGtPns;
                    _chartGiaTriNhapXuat.data.datasets[1].data = tongGtPxs;
                    _chartGiaTriNhapXuat.update();
                } else {
                    // Lấy thẻ canvas và vẽ biểu đồ tròn
                    var ctx = document.getElementById("giaTriNhapXuat").getContext("2d");
                    _chartGiaTriNhapXuat = new Chart(ctx, {
                        type: 'line',
                        data: data,
                        options: options
                    });
                }
            },
            error: function (loi) {
                console.log(loi);
            }
        });
    });
    $('#ngayGiaTriNhapXuat').change();

    // top 10 nhóm hàng bán chạy
    $('#ngayTop10NhomHangBanChay').on('change', function () {
        $.ajax({
            type: "post",
            url: "/QuanLy/BaoCaoLoiLo/getTop10NhomHangBanChay",
            data: "tg=" + $('#ngayTop10NhomHangBanChay').val(),
            success: function (result) {
                updateTableTop10NhomHangBanChay(result);
                const maValues = [];
                const slValues = [];
                for (const item of result) {
                    maValues.push(item.ma);
                    slValues.push(item.slXuat);
                }
                var data = {
                    labels: maValues,
                    datasets: [{
                        data: slValues, // Giá trị tương ứng với từng phần
                        backgroundColor: ["#FF6384", "#36A2EB", "#FFCE56", "#8BC34A", "#9C27B0", "#2196F3", "#FF9800", "#4CAF50", "#F44336", "#673AB7"],
                        hoverBackgroundColor: ["#FF6384", "#36A2EB", "#FFCE56", "#8BC34A", "#9C27B0", "#2196F3", "#FF9800", "#4CAF50", "#F44336", "#673AB7"],
                    }]
                };

                // Cấu hình biểu đồ
                var options = {
                    responsive: true,
                    maintainAspectRatio: false,
                    tooltips: {
                        callbacks: {
                            label: function (tooltipItem, data) {
                                var datasetLabel = data.labels[tooltipItem.index] || '';
                                
                                var value = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
                                return datasetLabel + ': ' + formatOddNumber(value);
                            }
                        }
                    }
                };

                // Kiểm tra xem đối tượng Chart đã được khởi tạo hay chưa
                if (_chartTop10NhomHangBanChay) {
                    _chartTop10NhomHangBanChay.data.labels = maValues;
                    _chartTop10NhomHangBanChay.data.datasets[0].data = slValues;
                    _chartTop10NhomHangBanChay.update();
                } else {
                    // Lấy thẻ canvas và vẽ biểu đồ tròn
                    var ctx = document.getElementById("top10NhomHangBanChay").getContext("2d");
                    _chartTop10NhomHangBanChay = new Chart(ctx, {
                        type: 'pie',
                        data: data,
                        options: options
                    });
                }
            },
            error: function (loi) {
                console.log(loi);
            }
        });
    });
    $('#ngayTop10NhomHangBanChay').change();

    // top 5 hàng hoá mang lại lợi nhuận
    $('#ngayTop5HangHoaDoanhThu').on('change', function () {
        $.ajax({
            type: "post",
            url: "/QuanLy/BaoCaoLoiLo/getTop5HangHoaDoanhThu",
            data: "tg=" + $('#ngayTop5HangHoaDoanhThu').val(),
            success: function (result) {
                updateTableTop5HangHoaDoanhThu(result);
                const maValues = [];
                const loiNhuanValues = [];
                for (const item of result) {
                    maValues.push(item.ma);
                    loiNhuanValues.push(item.loiNhuan.toFixed(2));
                }
                var data = {
                    labels: maValues,
                    datasets: [
                        {
                            data: loiNhuanValues,
                            backgroundColor: ["rgba(255, 99, 132, 0.5)", "rgba(54, 162, 235, 0.5)", "rgba(255, 206, 86, 0.5)", "rgba(139, 195, 74, 0.5)", "rgba(156, 39, 176, 0.5)"]
                        }
                    ]
                };

                // Cấu hình biểu đồ
                var options = {
                    responsive: true,
                    scales: {
                        r: {
                            pointLabels: {
                                display: true,
                                centerPointLabels: true,
                                font: {
                                    size: 18
                                }
                            }
                        }
                    },
                    plugins: {
                        legend: {
                            position: 'top',
                        },
                    },
                };

                // Kiểm tra xem đối tượng Chart đã được khởi tạo hay chưa
                if (_chartTop5HangHoaLoiNhuan) {
                    _chartTop5HangHoaLoiNhuan.data.labels = maValues;
                    _chartTop5HangHoaLoiNhuan.data.datasets[0].data = loiNhuanValues;
                    _chartTop5HangHoaLoiNhuan.update();
                } else {
                    // Lấy thẻ canvas và vẽ biểu đồ tròn
                    var ctx = document.getElementById("top5HangHoaDoanhThu").getContext("2d");
                    _chartTop5HangHoaLoiNhuan = new Chart(ctx, {
                        type: 'polarArea',
                        data: data,
                        options: options
                    });
                }
            },
            error: function (loi) {
                console.log(loi);
            }
        });
    });
    $('#ngayTop5HangHoaDoanhThu').change();


    $('#btnNhapXuat').on('click', function () {
        $.ajax({
            type: "post",
            url: "/QuanLy/BaoCaoLoiLo/download/nhapXuat",
            data: "tg=" + $('#ngayGiaTriNhapXuat').val(),
            xhrFields: {
                responseType: 'blob'
            },
            success: function (result) {
                var a = document.createElement('a');
                var url = window.URL.createObjectURL(result);
                a.href = url;
                a.download = "file.pdf";
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
    });

    $('#btnTop10').on('click', function () {
        $.ajax({
            type: "post",
            url: "/QuanLy/BaoCaoLoiLo/download/top10",
            data: "tg=" + $('#ngayTop10NhomHangBanChay').val(),
            xhrFields: {
                responseType: 'blob'
            },
            success: function (result) {
                var a = document.createElement('a');
                var url = window.URL.createObjectURL(result);
                a.href = url;
                a.download = "file.pdf";
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
    });
    $('#btnTop5').on('click', function () {
        $.ajax({
            type: "post",
            url: "/QuanLy/BaoCaoLoiLo/download/top5",
            data: "tg=" + $('#ngayTop5HangHoaDoanhThu').val(),
            xhrFields: {
                responseType: 'blob'
            },
            success: function (result) {
                var a = document.createElement('a');
                var url = window.URL.createObjectURL(result);
                a.href = url;
                a.download = "file.pdf";
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
    });
});
function getRandomColor() { //generates random colours and puts them in string
    var colors = [];
    for (var i = 0; i < 3; i++) {
        var letters = '0123456789ABCDEF'.split('');
        var color = '#';
        for (var x = 0; x < 6; x++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        colors.push(color);
    }
    return colors;
}
function updateTableTop10NhomHangBanChay(datas) {
    $('#table-top10NhomHangBanChay').empty();

    datas.forEach(function (data, tt) {
        $('#table-top10NhomHangBanChay').append(`<tr>
                                <td class="text-center">
                                    ${tt + 1}
                                </td>
                                <td class="text-center">
                                    ${data.ma}
                                </td>
                                <td style="max-width: 400px">
                                    ${data.ten}
                                </td>
                                <td class="text-end">
                                    ${formatOddNumber(data.slXuat)}
                                </td>
                                <td class="text-end">
                                    ${formatOddNumber(data.giaTriXuat)}
                                </td>
                            </tr>`)
    })
}
function updateTableGiaTriNhapXuat(datas) {
    $('#table-giaTriNhapXuat').empty();

    datas.forEach(function (data) {
        $('#table-giaTriNhapXuat').append(`<tr>
                                <td class="text-center">
                                    ${data.ngay}
                                </td>
                                <td class="text-end">
                                    ${formatOddNumber(data.tongPn) }
                                </td>
                                <td class="text-end">
                                    ${formatOddNumber(data.tongGtPn) }
                                </td>
                                <td class="text-end">
                                    ${formatOddNumber(data.tongPx)}
                                </td>
                                <td class="text-end">
                                    ${formatOddNumber(data.tongGtPx)}
                                </td>
                                <td class="text-end">
                                    ${formatOddNumber(data.loiNhuan)}
                                </td>
                            </tr>`)
    })
}
function updateTableTop5HangHoaDoanhThu(datas) {
    $('#table-top5HangHoaDoanhThu').empty();

    datas.forEach(function (data, tt) {
        $('#table-top5HangHoaDoanhThu').append(`<tr>
                                <td class="text-center">
                                    ${tt + 1}
                                </td>
                                <td class="text-center">
                                    ${data.ma}
                                </td>
                                <td style="max-width: 400px">
                                    ${data.ten}
                                </td>
                                <td class="text-end">
                                    ${formatOddNumber(data.slXuat)}
                                </td>
                                <td class="text-end">
                                    ${formatOddNumber(data.giaTriNhap)}
                                </td>
                                <td class="text-end">
                                    ${formatOddNumber(data.giaTriXuat)}
                                </td>
                                <td class="text-end">
                                    ${formatOddNumber(data.loiNhuan)}
                                </td>
                            </tr>`)
    })
}
// Hàm để chuyển đổi rgba sang hex
function rgbaToHex(rgba) {
    var values = rgba.substring(rgba.indexOf("(") + 1, rgba.lastIndexOf(")")).split(",");
    var hex = "#" + values.map(function (v) {
        var hexValue = parseInt(v).toString(16);
        return hexValue.length === 1 ? "0" + hexValue : hexValue;
    }).join("");
    return hex;
}

// Hàm để áp dụng độ trong suốt cho màu
function applyOpacityToColor(color, opacity) {
    var rgbaColor = Chart.helpers.color(color).alpha(opacity).rgbString();
    return rgbaToHex(rgbaColor);
}