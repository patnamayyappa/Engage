var CampusManagement;
(function (CampusManagement) {
    var LanguageMappings;
    (function (LanguageMappings) {
        var languages = {
            cultureInfo: {
                1025: { Locale: "ar-sa", CultureName: "ar-SA", LanguageRegion: "Arabic (Saudi Arabia)" },
                1069: { Locale: "eu", CultureName: "eu-ES", LanguageRegion: "Basque (Basque)" },
                1026: { Locale: "bg", CultureName: "bg-BG", LanguageRegion: "Bulgarian (Bulgaria)" },
                1027: { Locale: "ca", CultureName: "ca-ES", LanguageRegion: "Catalan (Catalan)" },
                2052: { Locale: "zh-cn", CultureName: "zh-CN", LanguageRegion: "Chinese (Simplified, PRC)" },
                3076: { Locale: "zh-tw", CultureName: "zh-HK", LanguageRegion: "Chinese (Traditional, Hong Kong S.A.R.)" },
                1028: { Locale: "zh-tw", CultureName: "zh-TW", LanguageRegion: "Chinese (Traditional, Taiwan)" },
                1050: { Locale: "hr", CultureName: "hr-HR", LanguageRegion: "Croatian (Croatia)" },
                1029: { Locale: "cs", CultureName: "cs-CZ", LanguageRegion: "Czech (Czech Republic)" },
                1030: { Locale: "da", CultureName: "da-DK", LanguageRegion: "Danish (Denmark)" },
                1043: { Locale: "nl", CultureName: "nl-NL", LanguageRegion: "Dutch (Netherlands)" },
                1033: { Locale: "en", CultureName: "en-US", LanguageRegion: "English (United States)" },
                1061: { Locale: "et", CultureName: "et-EE", LanguageRegion: "Estonian (Estonia)" },
                1035: { Locale: "fi", CultureName: "fi-FI", LanguageRegion: "Finnish (Finland)" },
                1036: { Locale: "fr", CultureName: "fr-FR", LanguageRegion: "French (France)" },
                1110: { Locale: "gl", CultureName: "gl-ES", LanguageRegion: "Galician (Galician)" },
                1031: { Locale: "de", CultureName: "de-DE", LanguageRegion: "German (Germany)" },
                1032: { Locale: "el", CultureName: "el-GR", LanguageRegion: "Greek (Greece)" },
                1037: { Locale: "he", CultureName: "he-IL", LanguageRegion: "Hebrew (Israel)" },
                1081: { Locale: "hi", CultureName: "hi-IN", LanguageRegion: "Hindi (India)" },
                1038: { Locale: "hu", CultureName: "hu-HU", LanguageRegion: "Hungarian (Hungary)" },
                1057: { Locale: "id", CultureName: "id-ID", LanguageRegion: "Indonesian (Indonesia)" },
                1040: { Locale: "it", CultureName: "it-IT", LanguageRegion: "Italian (Italy)" },
                1041: { Locale: "ja", CultureName: "ja-JP", LanguageRegion: "Japanese (Japan)" },
                1087: { Locale: "kk", CultureName: "kk-KZ", LanguageRegion: "Kazakh (Kazakhstan)" },
                1042: { Locale: "ko", CultureName: "ko-KR", LanguageRegion: "Korean (Korea)" },
                1062: { Locale: "lv", CultureName: "lv-LV", LanguageRegion: "Latvian (Latvia)" },
                1063: { Locale: "lt", CultureName: "lt-LT", LanguageRegion: "Lithuanian (Lithuania)" },
                1086: { Locale: "ms-my", CultureName: "ms-MY", LanguageRegion: "Malay (Malaysia)" },
                1044: { Locale: "nb", CultureName: "nb-NO", LanguageRegion: "Norwegian, Bokmål (Norway)" },
                1045: { Locale: "pl", CultureName: "pl-PL", LanguageRegion: "Polish (Poland)" },
                1046: { Locale: "pt-br", CultureName: "pt-BR", LanguageRegion: "Portuguese (Brazil)" },
                2070: { Locale: "pt", CultureName: "pt-PT", LanguageRegion: "Portuguese (Portugal)" },
                1048: { Locale: "ro", CultureName: "ro-RO", LanguageRegion: "Romanian (Romania)" },
                1049: { Locale: "ru", CultureName: "ru-RU", LanguageRegion: "Russian (Russia)" },
                3098: { Locale: "sr-cyrl", CultureName: "sr-Cyrl-CS", LanguageRegion: "Serbian (Cyrillic, Serbia and Montenegro (Former))" },
                2074: { Locale: "sr", CultureName: "sr-Latn-CS", LanguageRegion: "Serbian (Latin, Serbia and Montenegro (Former))" },
                1051: { Locale: "sk", CultureName: "sk-SK", LanguageRegion: "Slovak (Slovakia)" },
                1060: { Locale: "sl", CultureName: "sl-SI", LanguageRegion: "Slovenian (Slovenia)" },
                3082: { Locale: "es", CultureName: "es-ES", LanguageRegion: "Spanish (Spain, International Sort)" },
                1053: { Locale: "sv", CultureName: "sv-SE", LanguageRegion: "Swedish (Sweden)" },
                1054: { Locale: "th", CultureName: "th-TH", LanguageRegion: "Thai (Thailand)" },
                1055: { Locale: "tr", CultureName: "tr-TR", LanguageRegion: "Turkish (Turkey)" },
                1058: { Locale: "uk", CultureName: "uk-UA", LanguageRegion: "Ukrainian (Ukraine)" },
                1066: { Locale: "vi", CultureName: "vi-VN", LanguageRegion: "Vietnamese (Vietnam)" }
            }
        };
        function getLocale(lcid) {
            if (typeof languages.cultureInfo[lcid] == 'undefined') {
                return "";
            }
            else {
                return languages.cultureInfo[lcid].Locale;
            }
        }
        LanguageMappings.getLocale = getLocale;
        ;
        function getCultureName(lcid) {
            if (typeof languages.cultureInfo[lcid] == 'undefined') {
                return "";
            }
            else {
                return languages.cultureInfo[lcid].CultureName;
            }
        }
        LanguageMappings.getCultureName = getCultureName;
        ;
        function getLanguageRegion(lcid) {
            if (typeof languages.cultureInfo[lcid] == 'undefined') {
                return "";
            }
            else {
                return languages.cultureInfo[lcid].LanguageRegion;
            }
        }
        LanguageMappings.getLanguageRegion = getLanguageRegion;
        ;
    })(LanguageMappings = CampusManagement.LanguageMappings || (CampusManagement.LanguageMappings = {}));
})(CampusManagement || (CampusManagement = {}));
