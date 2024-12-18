// Set new default font family and font color to mimic Bootstrap's default styling
Chart.defaults.global.defaultFontFamily = 'Nunito', '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
Chart.defaults.global.defaultFontColor = '#858796';

let myLineChart; // Biến để lưu biểu đồ hiện tại

// Hàm lấy dữ liệu từ API
async function fetchData(startDate, endDate) {
    try {
        const url = new URL('/api/requests/monthly', window.location.origin);
        if (startDate) url.searchParams.append('startDate', startDate);
        if (endDate) url.searchParams.append('endDate', endDate);

        console.log("API URL:", url.toString());

        const response = await fetch(url);
        const data = await response.json();
        console.log("API Response:", data);

        return data; // Trả về dữ liệu JSON từ API
    } catch (error) {
        console.error("Error fetching data:", error);
        return null;
    }
}

// Hàm xử lý tạo biểu đồ
// Hàm xử lý tạo biểu đồ
async function createChart(startDate, endDate) {
    const data = await fetchData(startDate, endDate);

    // Kiểm tra dữ liệu từ API
    if (!data || !data.months || !data.requests) {
        console.error("No data available for the chart.");
        alert("No data available for the selected range.");
        return;
    }

    // Lấy danh sách tháng và số lượng yêu cầu từ API
    const months = data.months;
    const requests = data.requests;

    // Kiểm tra dữ liệu sau khi lấy từ API
    console.log("Months from API:", months);
    console.log("Requests from API:", requests);

    // Xác định chỉ số tháng bắt đầu và kết thúc dựa trên startDate và endDate
    const startMonthIndex = startDate ? months.findIndex(month => new Date(month).getMonth() === new Date(startDate).getMonth()) : 0;
    const endMonthIndex = endDate ? months.findIndex(month => new Date(month).getMonth() === new Date(endDate).getMonth()) : months.length - 1;

    // Kiểm tra các chỉ số
    console.log("Start Month Index:", startMonthIndex);
    console.log("End Month Index:", endMonthIndex);

    // Lọc dữ liệu từ API dựa trên khoảng thời gian được chọn
    const filteredMonths = months.slice(startMonthIndex, endMonthIndex + 1);
    const filteredRequests = requests.slice(startMonthIndex, endMonthIndex + 1);

    // Kiểm tra dữ liệu sau khi lọc
    console.log("Filtered Months:", filteredMonths);
    console.log("Filtered Requests:", filteredRequests);

    // Kiểm tra nếu không có dữ liệu sau khi lọc
    if (filteredMonths.length === 0 || filteredRequests.length === 0) {
        console.error("No data available for the selected range.");
        alert("No data available for the selected range.");
        return;
    }

    const ctx = document.getElementById("myAreaChart");
    if (myLineChart) {
        myLineChart.destroy(); // Hủy biểu đồ cũ trước khi tạo biểu đồ mới
    }

    // Tạo biểu đồ mới
    myLineChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: filteredMonths, // Hiển thị tháng
            datasets: [{
                label: "Requests",
                lineTension: 0.3,
                backgroundColor: "rgba(121, 28, 181, 0.05)",
                borderColor: "rgba(121, 28, 181, 1)",
                pointRadius: 3,
                pointBackgroundColor: "rgba(121, 28, 181, 1)",
                pointBorderColor: "rgba(121, 28, 181, 1)",
                pointHoverRadius: 3,
                pointHoverBackgroundColor: "rgba(121, 28, 181, 1)",
                pointHoverBorderColor: "rgba(121, 28, 181, 1)",
                pointHitRadius: 10,
                pointBorderWidth: 2,
                data: filteredRequests // Dữ liệu số lượng yêu cầu
            }]
        },
        options: {
            maintainAspectRatio: false,
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true,
                        stepSize: 2
                    }
                }]
            },
            legend: { display: false }
        }
    });
}

// Hàm xử lý sự kiện khi người dùng chọn ngày
document.getElementById('filterButton').addEventListener('click', function (event) {
    event.preventDefault(); // Ngăn reload trang khi nhấn nút

    const startDate = document.getElementById("startDate").value || getCurrentYearStartDate();
    const endDate = document.getElementById("endDate").value || getCurrentYearEndDate();

    console.log("Start Date:", startDate);
    console.log("End Date:", endDate);

    // Gọi hàm tạo biểu đồ với ngày bắt đầu và kết thúc
    createChart(startDate, endDate);
});

// Hàm lấy ngày đầu tiên của năm hiện tại
function getCurrentYearStartDate() {
    const currentYear = new Date().getFullYear();
    return `${currentYear}-01-01`;
}

// Hàm lấy ngày cuối cùng của năm hiện tại
function getCurrentYearEndDate() {
    const currentYear = new Date().getFullYear();
    return `${currentYear}-12-31`;
}

// Gọi biểu đồ mặc định khi trang tải
createChart(getCurrentYearStartDate(), getCurrentYearEndDate());
