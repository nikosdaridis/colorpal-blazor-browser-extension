chrome.runtime.onInstalled.addListener(({ reason }) => {
    if (reason === "install") {
        chrome.tabs.create({ url: "https://daridis.com/colorpal/onboarding", });
    }
});
