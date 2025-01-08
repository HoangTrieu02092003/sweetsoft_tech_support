document.addEventListener("DOMContentLoaded", function () {
    const chatOverlay = document.getElementById("chatOverlay");
    const closeChatButton = document.getElementById("closeChat");
    const chatBody = document.getElementById("chatBody");
    const chatTitle = document.getElementById("chatTitle");
    const requestIdInput = document.getElementById("requestIdInput");

    // Xử lý mở chat box
    document.querySelectorAll(".open-chat").forEach(button => {
        button.addEventListener("click", function (e) {
            e.stopPropagation();
            const requestId = this.getAttribute("data-request-id");
            const title = this.getAttribute("data-title");

            chatTitle.textContent = `${title}`;
            requestIdInput.value = requestId;

            chatBody.innerHTML = "<p>Đang tải...</p>";
            loadChatMessages(requestId);
            //
            document.body.classList.add('chat-open');

            //đánh dấu đã xem feedback
            markFeedbackAsRead(requestId);

            chatOverlay.classList.remove("hidden");
            setTimeout(() => chatOverlay.classList.add("visible"), 10);

            // Start polling for new feedbacks
            startPolling(requestId);
        });
    });

    // Hàm tải tin nhắn chat
    function loadChatMessages(requestId) {
        fetch(`/TblRequestsProcessings/GetFeedbacks?requestId=${requestId}`)
            .then(response => response.text())
            .then(html => {
                chatBody.innerHTML = html;
                scrollToBottom();
            })
            .catch(err => {
                chatBody.innerHTML = "<p>Lỗi khi tải phản hồi!</p>";
            });
    }

    // Hàm cuộn xuống cuối chat box
    function scrollToBottom() {
        chatBody.scrollTop = chatBody.scrollHeight;
    }

    // Xử lý đóng chat box
    closeChatButton.addEventListener("click", function () {
        chatOverlay.classList.remove("visible");
        setTimeout(() => chatOverlay.classList.add("hidden"), 300);
        document.body.classList.remove('chat-open');

        // Stop polling when chat is closed
        stopPolling();
    });

    // Xử lý form submit
    const chatForm = document.querySelector(".chat-form");
    const messageInput = document.getElementById("messageInput");

    chatForm.addEventListener("submit", function (e) {
        e.preventDefault();

        const formData = new FormData(chatForm);
        const requestId = requestIdInput.value;

        fetch(chatForm.action, {
            method: "POST",
            body: formData
        })
            .then(response => {
                if (!response.ok) throw new Error("Lỗi khi gửi tin nhắn");
                return response.text();
            })
            .then(html => {
                // Cập nhật nội dung chat và cuộn xuống
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');  // Chuyển chuỗi HTML thành đối tượng DOM
                const chatContent = doc.querySelector('.chatBody'); // Tìm phần tử với class "chat-content"
                chatBody.innerHTML = chatContent ? chatContent.innerHTML : "";
                loadChatMessages(requestId);
                messageInput.value = "";
                messageInput.style.height = "24px";
                scrollToBottom();


            })
            .catch(error => {
                console.error("Error:", error);
                chatBody.innerHTML += "<p>Lỗi khi gửi tin nhắn!</p>";
            });
    });

    // Xử lý auto-resize cho textarea
    messageInput.addEventListener("input", function () {
        this.style.height = "24px";
        const newHeight = Math.min(this.scrollHeight, 100);
        this.style.height = newHeight + "px";
    });

    // Hàm polling để tải lại tin nhắn chat mỗi 5 giây
    let pollingInterval;

    function startPolling(requestId) {
        pollingInterval = setInterval(() => loadChatMessages(requestId), 5000);
    }

    function stopPolling() {
        clearInterval(pollingInterval);
    }

});

// Hàm gọi AJAX để cập nhật trạng thái đã đọc
function markFeedbackAsRead(requestId) {
    fetch(`/TblRequestsProcessings/MarkAsRead?requestId=${requestId}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-CSRF-TOKEN": document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => {
            if (!response.ok) throw new Error("Failed to mark feedback as read");
            // Nếu thành công, xóa lớp "has-unread-feedback" khỏi button
            const button = document.querySelector(`button[data-request-id="${requestId}"]`);
            if (button) {
                button.classList.remove("has-unread-feedback");
            }
        })
        .catch(error => console.error("Error marking feedback as read:", error));
}
