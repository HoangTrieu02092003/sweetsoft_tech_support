document.getElementById("filterButton").addEventListener("click", async function (event) {
    event.preventDefault(); // Ngăn không cho hành động mặc định

    // Lấy giá trị ngày bắt đầu và ngày kết thúc từ các input
    const startDate = document.getElementById("startDate").value;
    const endDate = document.getElementById("endDate").value;

    // Nếu không có ngày, hiển thị 12 tháng trong năm hiện tại
    if (!startDate || !endDate) {
        await renderDefaultChart();
    } else {
        // Kiểm tra nếu ngày bắt đầu và ngày kết thúc hợp lệ
        if (new Date(startDate) > new Date(endDate)) {
            alert("Start date cannot be later than end date.");
            return;
        }
        // Gọi hàm để fetch và render biểu đồ với dữ liệu mới
        await fetchAndRenderChart(startDate, endDate, false);
    }
});

async function fetchAndRenderChart(startDate, endDate, isDefault) {
    try {
        const response = await fetch(`/api/requests/monthly?startDate=${startDate}&endDate=${endDate}`);
        if (!response.ok) {
            throw new Error("Failed to fetch data from API");
        }

        const data = await response.json();

        if (!data.monthlySummary || data.monthlySummary.length === 0) {
            alert("No data available for the selected date range.");
            return;
        }

        let labels = [];
        let counts = [];

        if (isDefault) {
            const currentYear = new Date().getFullYear();
            for (let i = 1; i <= 12; i++) {
                labels.push(new Date(currentYear, i - 1).toLocaleString('default', { month: 'long' }));
                counts.push(0);
            }
        } else {
            const start = new Date(startDate);
            const end = new Date(endDate);
            const months = getMonthsBetween(start, end);

            labels = months.map(month => month.toLocaleString('default', { month: 'long' }));
            counts = new Array(months.length).fill(0);

            data.monthlySummary.forEach(item => {
                const monthIndex = months.findIndex(month =>
                    month.getMonth() === item.month - 1 &&
                    month.getFullYear() === item.year
                );
                if (monthIndex !== -1) {
                    counts[monthIndex] = item.count;
                }
            });
        }

        console.log("Labels:", labels);
        console.log("Counts:", counts);

        if (myLineChart) {
            myLineChart.destroy();
        }

        drawChart(labels, counts);

    } catch (error) {
        console.error("Error fetching data:", error);
        alert("An error occurred while fetching data.");
    }
}

async function renderDefaultChart() {
    const currentYear = new Date().getFullYear();
    const defaultStartDate = `${currentYear}-01-01`;
    const defaultEndDate = `${currentYear}-12-31`;

    let labels = [];
    let counts = [];

    for (let i = 1; i <= 12; i++) {
        labels.push(new Date(currentYear, i - 1).toLocaleString('default', { month: 'long' }));
        counts.push(0);
    }

    try {
        const response = await fetch(`/api/requests/monthly?startDate=${defaultStartDate}&endDate=${defaultEndDate}`);
        if (!response.ok) {
            throw new Error("Failed to fetch data from API");
        }

        const data = await response.json();

        if (data.monthlySummary && data.monthlySummary.length > 0) {
            data.monthlySummary.forEach(item => {
                const monthIndex = labels.findIndex(label =>
                    new Date(currentYear, item.month - 1).toLocaleString('default', { month: 'long' }) === label
                );
                if (monthIndex !== -1) {
                    counts[monthIndex] = item.count;
                }
            });
        }

        drawChart(labels, counts);
    } catch (error) {
        console.error("Error fetching default data:", error);
        alert("An error occurred while fetching default data.");
    }
}

function getMonthsBetween(startDate, endDate) {
    const months = [];
    const start = new Date(startDate);
    const end = new Date(endDate);

    while (start <= end) {
        months.push(new Date(start));
        start.setMonth(start.getMonth() + 1);
    }

    return months;
}

function drawChart(labels, data) {
    const ctx = document.getElementById("myAreaChart").getContext("2d");

    myLineChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
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
                data: data,
            }],
        },
        options: {
            maintainAspectRatio: false,
            scales: {
                x: {
                    type: 'category',
                    labels: labels,
                    grid: { display: false },
                },
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 2,
                    },
                    grid: {
                        color: "rgb(234, 236, 244)",
                    }
                }
            }
        }
    });
}

let myLineChart;

// Tự động vẽ biểu đồ mặc định khi trang tải
window.addEventListener("DOMContentLoaded", async function () {
    await renderDefaultChart();
});
