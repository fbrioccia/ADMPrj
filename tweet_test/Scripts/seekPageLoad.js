var enableDisableNextBtn = function ()
{
        $(".btn-lg").prop("disabled", $("a[class$=active]").length <= 0);
}
var selectedTweets = [];
var tweetsOnPage = {};

var loadAccountTweetOnPage = function (data) {
    $(".loop").empty();
    tweetsOnPage = data;

    for (var i = 0; i < data.length; i++) {

        $(".loop").append(
            "<a href='javascript:void();' class='list-group-item' tweetId = '" + i + "' onclick='elementClick(this);'>"
            + "    <h4 class='list-group-item-heading'>"
            + data[i].account.Name
            + "                        <img class='img-thumbnail img-rounded' style='float:right; max-width:50px; max-height:50px;' src='" + data[i].account.ImageUrl + "' />"
            + "    </h4>"
            + "    <p class='list-group-item-text'> " + data[i].tweet.Text + " </p>"
            + "</a>");
    }
};

$(document).ready(function () {
    $('#srch-term').keypress(function (e) { 
        if (e.which == 13) {//Enter key pressed
            $('#search-tweets-button').click();//Trigger search button click event
        }
    });
});

var searchClick = function () {
    $(".btn-lg").prop("disabled", true);
    selectedTweets = [];
    $.ajax({
        url: 'SeekSearch',
        contentType: "application/json; charset=utf-8",
        data: { "searchTerms": $("input[name*='srch-term']").val() },
        success: function (data) {
            return loadAccountTweetOnPage(data);
        },
        error: function (jqxhr, textStatus, errorThrown) {
        }
    });
};

var elementClick = function (element) {
    var clicked = $(element);
    if (clicked.hasClass('active')) {
        clicked.removeClass('active');
        selectedTweets = selectedTweets.filter(function (x) { return x != clicked.attr("tweetId"); });
    }
    else {
        clicked.addClass('active');
        selectedTweets.push(clicked.attr("tweetId"));
    }
    enableDisableNextBtn();
};

var nextClick = function () {
    var tweetWrap = tweetsOnPage.filter(function (x) { return selectedTweets.includes("" + tweetsOnPage.indexOf(x)) });
    $.ajax({
        type: "POST",
        url: 'TweetsSave',
        contentType: "application/json",
        data: JSON.stringify({ SeekResults: tweetWrap }),
        contentType: "application/json;charset=utf-8"
    }).done(function (res) {
        window.location.href = res.newUrl;
    }).fail(function (xhr, a, error) {
        console.log(error);
    });
};