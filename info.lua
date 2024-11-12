local buildDirectory = ".build"

local info = {
    appName = "GMTK24",
    itchUrl = "architower",
    iconPath = "GMTK24/Icon.bmp",
    buildDirectory = buildDirectory,

    platformToProject =
    {
        ["macos-universal"] = "GMTK24",
        ["win-x64"] = "GMTK24",
        ["linux-x64"] = "GMTK24",
    },

    butlerChannelForPlatform = function(platform)
        return "latest-" .. platform
    end,

    buildDirectoryForPlatform = function(platform)
        return buildDirectory .. '/' .. platform
    end
}

return info