document.getElementById('filterButton').addEventListener('click', function (event) {
    event.preventDefault();  // Ngăn chặn reload trang khi nhấn nút

    // Lấy giá trị từ form ngày bắt đầu và kết thúc
    const startDate = document.getElementById("startDate").value || getCurrentYearStartDate();
    const endDate = document.getElementById("endDate").value || getCurrentYearEndDate();

    // Gọi hàm tạo biểu đồ với startDate và endDate
    createChart(startDate, endDate);
});

// Hàm lấy ngày đầu tiên của năm hiện tại
function getCurrentYearStartDate() {
    const currentYear = new Date().getFullYear();
    return `${currentYear}-01-01`;  // Định dạng yyyy-mm-dd
}

// Hàm lấy ngày cuối cùng của năm hiện tại
function getCurrentYearEndDate() {
    const currentYear = new Date().getFullYear();
    return `${currentYear}-12-31`;  // Định dạng yyyy-mm-dd
}

async function fetchData(startDate, endDate) {
    try {
        // Tạo URL với tham số startDate và endDate
        const url = new URL('/api/requests/status-summary', window.location.origin);
        if (startDate) url.searchParams.append('startDate', startDate);
        if (endDate) url.searchParams.append('endDate', endDate);

        const response = await fetch(url); // Gọi API từ controller mới
        const data = await response.json(); // Parse dữ liệu JSON
        console.log("API from status",data); // Kiểm tra dữ liệu trả về
        return data;
    } catch (error) {
        console.error("Error fetching data:", error);
    }
}

let myBarChart; // Khai báo biến toàn cục để lưu biểu đồ hiện tại

async function createChart(startDate, endDate) {
    // Gọi fetchData với startDate và endDate
    const data = await fetchData(startDate, endDate);

    if (!data || !data.requests) {
        console.error("No data available for chart.");
        return;
    }

    // Tạo một mảng đếm theo trạng thái và khởi tạo với 0
    const statusCounts = { 1: 0, 2: 0, 3: 0, 4: 0 };

    // Gán giá trị từ API trả về
    data.requests.forEach(request => {
        if (statusCounts.hasOwnProperty(request.status)) {
            statusCounts[request.status] = request.count;
        }
    });

    const ctx = document.getElementById("myBarChart");

    // Kiểm tra nếu đã có biểu đồ, thì hủy biểu đồ cũ trước khi tạo mới
    if (myBarChart) {
        myBarChart.destroy();
    }

    // Tạo biểu đồ mới
    myBarChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Pending', 'Processing', 'Completed', 'Cannot be Resolved'], // Trục X
            datasets: [{
                label: "Number of Requests",
                backgroundColor: [
                    "rgba(255, 99, 132, 0.5)",   // Pending
                    "rgba(255, 159, 64, 0.5)",   // Processing
                    "rgba(121, 28, 181, 0.5)",   // Completed
                    "rgba(75, 192, 192, 0.5)"    // Cannot be Resolved
                ],
                borderColor: [
                    "rgba(255, 99, 132, 1)",
                    "rgba(255, 159, 64, 1)",
                    "rgba(121, 28, 181, 1)",
                    "rgba(75, 192, 192, 1)"
                ],
                data: [
                    statusCounts[1], // Pending
                    statusCounts[2], // Processing
                    statusCounts[3], // Completed
                    statusCounts[4]  // Cannot be Resolved
                ],
                borderWidth: 1
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
            legend: { display: false },
            hover: {
                mode: 'nearest', // Bật hover (có thể là 'index' hoặc 'nearest')
                intersect: true  // Chỉ hiển thị hover khi trỏ trực tiếp vào điểm dữ liệu
            },
            tooltips: {
                enabled: true,  // Bật tooltips
                mode: 'index',  // Hiển thị tooltip cho tất cả dataset tại vị trí x
                intersect: false, // Cho phép hiển thị tooltip ngay cả khi không hover trực tiếp vào cột
                callbacks: {
                    label: function (tooltipItem, data) {
                        return `${data.datasets[tooltipItem.datasetIndex].label}: ${tooltipItem.yLabel}`;
                    }
                }
            }
        }
    });
}


// Gọi createChart khi trang tải để hiển thị dữ liệu mặc định của năm hiện tại
createChart(getCurrentYearStartDate(), getCurrentYearEndDate());
