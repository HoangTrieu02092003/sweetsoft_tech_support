using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace admin_sweetsoft_tech_support.Attributes
{
    public static class HtmlHelpers
    {
        public static IHtmlContent GenerateFilterDropdown(
    this IHtmlHelper htmlHelper,
    string id,
    string name,
    Dictionary<string, string> options,
    string selectedValue = "",
    string placeholder = "Tất cả",
    string cssClass = "form-select",
    bool includeCancelButton = true,
    string cancelButtonLabel = "X")
        {
            var containerTag = new TagBuilder("div");
            containerTag.AddCssClass("input-fillter-container");

            // Tạo thẻ <select>
            var selectTag = new TagBuilder("select");
            selectTag.Attributes["id"] = id;
            selectTag.Attributes["name"] = name;
            selectTag.Attributes["class"] = cssClass;
            selectTag.Attributes["onchange"] = "submitForm()";

            // Thêm tùy chọn Placeholder
            var placeholderOption = new TagBuilder("option");
            placeholderOption.Attributes["value"] = "";
            placeholderOption.InnerHtml.Append(placeholder);
            if (string.IsNullOrEmpty(selectedValue))
            {
                placeholderOption.Attributes["selected"] = "selected";
            }
            selectTag.InnerHtml.AppendHtml(placeholderOption);

            // Thêm các tùy chọn khác
            foreach (var option in options)
            {
                var optionTag = new TagBuilder("option");
                optionTag.Attributes["value"] = option.Key;
                optionTag.InnerHtml.Append(option.Value);
                if (option.Key == selectedValue)
                {
                    optionTag.Attributes["selected"] = "selected";
                }
                selectTag.InnerHtml.AppendHtml(optionTag);
            }

            // Nút hủy lọc
            TagBuilder cancelButtonTag = null;
            if (includeCancelButton)
            {
                cancelButtonTag = new TagBuilder("button");
                cancelButtonTag.Attributes["type"] = "button";
                cancelButtonTag.Attributes["class"] = "btn btn-sm btn-danger";
                cancelButtonTag.Attributes["onclick"] = $"clearFilter('{id}')"; // Xóa giá trị ô dropdown
                cancelButtonTag.InnerHtml.Append(cancelButtonLabel);
            }

            // Thêm thẻ select vào container
            containerTag.InnerHtml.AppendHtml(selectTag);

            // Nếu có nút hủy, thêm vào container
            if (includeCancelButton && cancelButtonTag != null)
            {
                containerTag.InnerHtml.AppendHtml(cancelButtonTag);
            }

            // Tạo script JavaScript
            var scriptTag = new TagBuilder("script");
            scriptTag.InnerHtml.AppendHtml(@"
        function submitForm() {
            document.getElementById('" + id + @"').form.submit();
        }
        function clearFilter(selectId) {
            document.getElementById(selectId).value = '';
            document.getElementById(selectId).form.submit();
        }
    ");

            // Kết hợp các phần tử
            var result = new HtmlContentBuilder();
            result.AppendHtml(containerTag);
            result.AppendHtml(scriptTag);

            return result;
        }


        //
        public static IHtmlContent GenerateFilterTextbox(
    this IHtmlHelper htmlHelper,
    string id,
    string name,
    string placeholder = "Nhập để tìm kiếm...",
    string cssClass = "form-control",
    string value = "",
    bool includeCancelButton = true,
    string cancelButtonLabel = "X")
        {
            var containerTag = new TagBuilder("div");
            containerTag.AddCssClass("input-fillter-container");

            // Tạo thẻ <input>
            var inputTag = new TagBuilder("input");
            inputTag.Attributes["type"] = "text";
            inputTag.Attributes["id"] = id;
            inputTag.Attributes["name"] = name;
            inputTag.Attributes["class"] = cssClass;
            inputTag.Attributes["placeholder"] = placeholder;
            inputTag.Attributes["value"] = value;
            inputTag.Attributes["oninput"] = "autoSubmitForm()";

            // Thêm thẻ input vào container
            containerTag.InnerHtml.AppendHtml(inputTag);

            // Nút hủy lọc
            TagBuilder cancelButtonTag = null;
            if (includeCancelButton)
            {
                cancelButtonTag = new TagBuilder("button");
                cancelButtonTag.Attributes["type"] = "button";
                cancelButtonTag.Attributes["class"] = "btn btn-sm btn-danger";
                cancelButtonTag.Attributes["onclick"] = $"clearFilter('{id}')"; // Xóa giá trị ô input
                cancelButtonTag.InnerHtml.Append(cancelButtonLabel);
                containerTag.InnerHtml.AppendHtml(cancelButtonTag);
            }

            // Tạo script JavaScript
            var scriptTag = new TagBuilder("script");
            scriptTag.InnerHtml.AppendHtml(@"
        function autoSubmitForm() {
            clearTimeout(window.autoSubmitTimer);
            window.autoSubmitTimer = setTimeout(function() {
                var value = document.getElementById('" + id + @"').value;
                var name = '" + name + @"';
                // Gửi AJAX yêu cầu với giá trị input
                $.ajax({
                    url: '/your-controller/your-action', // Thay đổi URL theo controller và action của bạn
                    type: 'GET',
                    data: { name: name, value: value },
                    success: function(response) {
                        // Xử lý dữ liệu trả về (cập nhật UI hoặc danh sách)
                        $('#your-results-container').html(response); // Cập nhật phần tử UI với dữ liệu mới
                    }
                });
            }, 300); // Delay 300ms để tránh submit quá nhanh
        }

        function clearFilter(inputId) {
            document.getElementById(inputId).value = '';
            autoSubmitForm(); // Gửi lại yêu cầu AJAX khi xóa
        }
    ");

            // Kết hợp các phần tử
            var result = new HtmlContentBuilder();
            result.AppendHtml(containerTag); // Gắn toàn bộ phần tử vào đây
            result.AppendHtml(scriptTag);

            return result;
        }


        // ngày
        public static IHtmlContent GenerateDateFilterTextbox(
    this IHtmlHelper htmlHelper,
    string id,
    string name,
    string placeholder = "Chọn ngày...",
    string cssClass = "form-control",
    string value = "",
    bool includeCancelButton = true,
    string cancelButtonLabel = "X")
        {
            var containerTag = new TagBuilder("div");
            containerTag.AddCssClass("input-fillter-container");

            // Tạo thẻ <input>
            var inputTag = new TagBuilder("input");
            inputTag.Attributes["type"] = "date";  // Chỉ định kiểu là date
            inputTag.Attributes["id"] = id;
            inputTag.Attributes["name"] = name;
            inputTag.Attributes["class"] = cssClass;
            inputTag.Attributes["placeholder"] = placeholder;
            inputTag.Attributes["value"] = value;
            inputTag.Attributes["oninput"] = "autoSubmitForm()";

            // Thêm thẻ input vào container
            containerTag.InnerHtml.AppendHtml(inputTag);

            // Nút hủy lọc
            TagBuilder cancelButtonTag = null;
            if (includeCancelButton)
            {
                cancelButtonTag = new TagBuilder("button");
                cancelButtonTag.Attributes["type"] = "button";
                cancelButtonTag.Attributes["class"] = "btn btn-sm btn-danger";
                cancelButtonTag.Attributes["onclick"] = $"clearFilter('{id}')";
                cancelButtonTag.InnerHtml.Append(cancelButtonLabel);
                containerTag.InnerHtml.AppendHtml(cancelButtonTag);
            }

            // Tạo script JavaScript
            var scriptTag = new TagBuilder("script");
            scriptTag.InnerHtml.AppendHtml(@"
        function autoSubmitForm() {
            document.getElementById('" + id + @"').form.submit();
        }
        function clearFilter(inputId) {
            document.getElementById(inputId).value = '';
            autoSubmitForm(); 
        }
    ");

            // Kết hợp các phần tử
            var result = new HtmlContentBuilder();
            result.AppendHtml(containerTag);
            result.AppendHtml(scriptTag);

            return result;
        }


    }
}
