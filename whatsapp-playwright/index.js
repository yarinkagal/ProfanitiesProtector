const { chromium } = require('playwright');

(async () => {
    // Launch the browser
    const browser = await chromium.launch({ headless: false });
    const context = await browser.newContext();

    // Open a new page
    const page = await context.newPage();

    // Navigate to WhatsApp Web
    await page.goto('https://web.whatsapp.com/');

    // Wait for the QR code to be visible
    await page.waitForSelector('canvas[aria-label="Scan this QR code to link a device!"]');

    // Prompt user to scan QR code with their phone
    console.log('Please scan the QR code with your phone.');

    // Wait for the user to scan the QR code and for the chat list to be visible
    await page.waitForSelector('#pane-side');

    await page.waitForTimeout(10000);

    // Get all chat list items
    const chatListItems = await page.$$('#pane-side [role="listitem"]');
    const limitedChatListItems = chatListItems.slice(0, 5);

    // create output for API
    const requestBody = [];

    for (const chatItem of limitedChatListItems) {
        // Click on each chat item
        await chatItem.click();
        // for stable
        await page.waitForTimeout(3000);

        // Wait for the chat to open
        await page.waitForSelector('.focusable-list-item');

        // Extract the chat title
        const chatTitle = await page.evaluate((chatItem) => {
            const parentElement = chatItem.querySelector('span[dir="auto"]');
            return parentElement ? parentElement.title : null;
        }, chatItem);

        // Extract incoming messages
        const messagesIn = await page.evaluate(() => {
            let messageElements = document.querySelectorAll('.message-in .copyable-text');
            let messageTexts = Array.from(messageElements).map(el => el.innerText);
            return [...new Set(messageTexts)];
        });

        // Extract outgoing messages
        const messagesOut = await page.evaluate(() => {
            let messageElements = document.querySelectorAll('.message-out .copyable-text');
            let messageTexts = Array.from(messageElements).map(el => el.innerText);
            return [...new Set(messageTexts)];
        });

        // Extract images
        const imagesIn = await page.evaluate(() => {
            let imageElements = document.querySelectorAll('.message-in img');
            let imageSources = Array.from(imageElements).map(el => el.src);
            return [...new Set(imageSources)];
        });

        const imagesOut = await page.evaluate(() => {
            let imageElements = document.querySelectorAll('.message-out img');
            let imageSources = Array.from(imageElements).map(el => el.src);
            return [...new Set(imageSources)];
        });

        // Download and encode images
        const encodedImagesIn = await getEncodedImages(imagesIn);
        const encodedImagesOut = await getEncodedImages(imagesOut);

        requestBody.push({ 'ChatName': chatTitle, 'MessagesIn': messagesIn, 'MessagesOut': messagesOut, 'ImagesIn': encodedImagesIn, 'ImagesOut': encodedImagesOut });
    }
    console.log('requestBody', requestBody);


    // Send POST request to the API endpoint
    const response = await fetch('https://profanitiesprotector.azurewebsites.net/ProfanitiesProtector/analyze', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            "Content": requestBody,
            "Email": "aaa@gmail.com"
        }
        )
    });

    // Close the browser
    await browser.close();
})();

async function getEncodedImages(images) {
    const encodedImages = [];
    for (const imageUrl of images) {
        try {
            const imageResponse = await fetch(imageUrl);
            const arrayBuffer = await imageResponse.arrayBuffer();
            const buffer = Buffer.from(arrayBuffer);
            const base64Image = buffer.toString('base64');
            encodedImages.push(`data:image/png;base64,${base64Image}`);
        } catch (err) {
            console.error(`Failed to download or encode image: ${err}`);
        }
    }
    return encodedImages;
}
