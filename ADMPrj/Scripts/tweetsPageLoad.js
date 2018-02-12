var tweetsDictionary = {}

var loadTweets = function () {
    $.ajax({
        url: 'TweetsLoad',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            debugger;
            tweetsDictionary = data;
            loadAccountTweetOnPage(tweetsDictionary);
        },
        error: function (ts) { alert(ts.responseText) }
    });
};

var addClickEventOnImages = function () {
    debugger;
    for (var i = 0; i < $(".scoreFace").length; i++) {
        $($(".scoreFace").get(i)).click(clickFunction);
    }
};

var clickFunction = function () {
    var clickedElement = $(this); 
    if (clickedElement.attr("src").includes("no_face")) {
        $.ajax({
            url: 'ComputeScore',
            contentType: "application/json; charset=utf-8",
            data: { "tweetId": tweetsDictionary[clickedElement.attr("id")].Tweet[0].Id },
            success: function (data) {
                debugger;
                changeFaceNPercentage(data, clickedElement);
            },
            error: function (jqxhr, textStatus, errorThrown) {
            }
        });
    }
};

$(document).ready(function () {
    loadTweets();
});

var loadAccountTweetOnPage = function (data) {
    $(".loop").empty();
    debugger;
    for (var i = 0; i < data.length; i++) {
        //debugger;
        $(".loop").append(
            "<div class='row'>"
            + "    <div class='col-md-10'>"
            + "        <a href='javascript:void();' class='list-group-item' tweetId='" + i + "' onclick='elementClick(this);'>"
            + "            <h4 class='list-group-item-heading'>"
            + data[i].TweetAccount.Name
            + "                        <img class='img-thumbnail id='"+i+"' img-rounded' style='float:right; max-width:50px; max-height:50px;' src='" + data[i].TweetAccount.ImageUrl + "' />"
            + "            </h4>"
            + "            <p class='list-group-item-text'>" + data[i].Tweet[0].Text + "</p>"
            + "        </a>"
            + "    </div>"
            + "    <div class='col-md-1'><input class='scoreFace' id='"+i+"' type='image' src='/Content/Images/" + getScoreFace(data[i].Tweet[0].Score) + "' /></div>"
            + "    <div class='col-md-1 percentageClass' id='" + i +"'>" + ((data[i].Tweet[0].Percentage == null) ? '' : data[i].Tweet[0].Percentage+'%') + "</div>"
            + "</div>");
    }

    addClickEventOnImages();
};

var getScoreFace = function (score) {
    if (score == null)
        return "no_face_64x64.png";
    else if (score == 0)
        return "emoticon_straight_face_64x64.png"
    else if (score > 0)
        return "emoticon_smile_64x64.png";
    else
        return "emoticon_frown_64x64.png";
};


var changeFaceNPercentage = function (score, clickedElement) {
    debugger;
    clickedElement.attr('src', clickedElement.attr('src').substring(0, clickedElement.attr('src').lastIndexOf("/") + 1) + getScoreFace(score.Item1));
    $("#"+clickedElement.get(0).id+".percentageClass").text(score.Item2+"%")
}