function toggleContent(processId) { 
    const details = document.getElementById(`details-${processId}`); 
    const icon = document.querySelector(`.toggle-icon[data-id='${processId}']`);
    const allDetails = document.querySelectorAll('[id^="details-"]');
    const allIcons = document.querySelectorAll('.toggle-icon');
    allDetails.forEach(detail => {
        if (detail !== details) {
            detail.style.maxHeight = '0px';
            detail.style.padding = '0 15px';
        }
    });
    allIcons.forEach(otherIcon => {
        if (otherIcon !== icon) {
            otherIcon.style.transform = 'rotate(0deg)';
        }
    });
    if (details.style.maxHeight === '0px' || !details.style.maxHeight) { 
        details.style.maxHeight = details.scrollHeight + 'px'; 
        details.style.padding = '0 15px';
        icon.style.transform = 'rotate(180deg)';
        setTimeout(() => {
            const rect = details.getBoundingClientRect();
            const offset = rect.top + window.scrollY - 250;
            window.scrollTo({ top: offset, behavior: 'smooth' });
        }, 300);
    } else { 
        details.style.maxHeight = '0px'; 
        details.style.padding = '0 15px'; 
        icon.style.transform = 'rotate(0deg)';
    } 
}



document.addEventListener("DOMContentLoaded", () => {
    const tabs = document.querySelectorAll(".zc ul li");
    const requests = document.querySelectorAll(".container");

    tabs.forEach((tab, index) => {
        tab.addEventListener("click", () => {
            // Bỏ class 'on' khỏi tất cả các tab
            tabs.forEach((item) => item.classList.remove("on"));
            // Thêm class 'on' cho tab được chọn
            tab.classList.add("on");

            // Xác định trạng thái cần lọc
            let status;
            switch (index) {
                case 0:
                    status = -1; // Hiển thị tất cả
                    break;
                case 1:
                    status = 0; // Đang xử lý
                    break;
                case 2:
                    status = 1; // Hoàn thành
                    break;
                case 3:
                    status = 2; // Không xử lý được
                    break;
                default:
                    status = -1;
            }

            // Lọc các yêu cầu
            requests.forEach((request) => {
                const requestStatus = parseInt(request.querySelector(".status").dataset.status);
                if (status === -1 || requestStatus === status) {
                    request.style.display = "block"; // Hiển thị
                } else {
                    request.style.display = "none"; // Ẩn
                }
            });
        });
    });
});



(function ($) {
    if (!$) return;

    $('.inner_main .rem12 .zb_nr .zc ul li ul li a').click(function (e) {
        e.preventDefault();
        const target = $(this).attr('href');
        if (target && $(target).length) {
            $('html,body').animate({
                scrollTop: $(target).offset().top - 138
            }, 500);
        }
    });
})(jQuery);


const RequestHandler = (() => {
    let inner_dh_kg = 0;

    function handleListMoreClick() {
        document.getElementById("listmore-toggle").addEventListener('click', () => {
            const list = document.querySelector('.zc ul');
            const listmore = document.querySelector('.listmore');
            const closeIcon = document.querySelector('.listmore .close');
            const dotIcon = document.querySelector('.listmore .icon-dot-3');

            if (inner_dh_kg === 0) {
                if (list) list.style.display = 'block';
                if (listmore) {
                    listmore.style.borderBottom = '0';
                    listmore.style.marginBottom = '-2px';
                }
                if (closeIcon) closeIcon.style.display = 'block';
                if (dotIcon) dotIcon.style.display = 'none';
                inner_dh_kg = 1;
            } else {
                if (list) list.style.display = 'none';
                if (listmore) {
                    listmore.style.borderBottom = '1px solid #ddd';
                    listmore.style.marginBottom = '0';
                }
                if (closeIcon) closeIcon.style.display = 'none';
                if (dotIcon) dotIcon.style.display = 'block';
                inner_dh_kg = 0;
            }
        });
    }

    return { handleListMoreClick };
})();

document.addEventListener("DOMContentLoaded", () => {
    RequestHandler.handleListMoreClick();
});
