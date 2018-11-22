$('<link>')
			.appendTo($('head'))
			.attr({type : 'text/css', rel : 'stylesheet'})
			.attr('href', 'Content/Resources/Stylesheets/NavPanel_OpenSans.css');

			$(document).ready(function(){
			$(".button-group-container-left").text(document.title);
			$(".button-group-container-left").css({"font-family":"Open Sans Light Regular", "font-size":"18pt", "margin-left":"4px", color:"#1f1f1f"});
			$(".button-group-container-left").offset({top:52})
			});
		