mergeInto(LibraryManager.library, {
    SetGamePlayListener: function (Name, OnGamePause, OnGameResume) {
        Name = UTF8ToString(Name);
        OnGamePause = UTF8ToString(OnGamePause);
        OnGameResume = UTF8ToString(OnGameResume);
        SetPlayListener(Name, OnGamePause, OnGameResume);
    },

    IsMobileDevice: function () {
        if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
            return 1;
        } else {
            return 0;
        }
    },
	IsAndroidDevice: function () {
    	if (/Android/i.test(navigator.userAgent)) {
        return 1;
    	} else {
        return 0;
    	}
	},

    IsInternetConnectionAvailable: function() {        
        return navigator.onLine
    }
});