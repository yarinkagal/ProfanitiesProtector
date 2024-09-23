import { chromium } from 'playwright';
import express from 'express';
import path from 'path';
import fetch from 'node-fetch';
import { fileURLToPath } from 'url';
import { Buffer } from 'buffer';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const app = express();
const port = 3000;

app.use(express.static('public'));
app.use(express.json());

app.post('/submit-email', async (req, res) => {
    const email = req.body.email;

    if (!email) {
        return res.status(400).json({ message: 'Email is required' });
    }

    // Your Playwright script logic here
    (async () => {
        const browser = await chromium.launch({ headless: false });
        const context = await browser.newContext();
        const page = await context.newPage();

        await page.goto('https://web.whatsapp.com/');

        await page.waitForSelector('canvas[aria-label="Scan this QR code to link a device!"]');
        console.log('Please scan the QR code with your phone.');

        await page.waitForSelector('#pane-side', { timeout: 60000 });

        await page.waitForTimeout(10000);

        const chatListItems = await page.$$('#pane-side [role="listitem"]');
        const limitedChatListItems = chatListItems.slice(0,8);

        const requestBody = [];

        for (const chatItem of limitedChatListItems) {
            await chatItem.click();
            await page.waitForTimeout(3000);
            await page.waitForSelector('.focusable-list-item');          

            const chatTitle = await page.evaluate(item => {
                const parent = item.querySelector('span[dir="auto"]');
                return parent ? parent.title : null;
            }, chatItem);

            const messagesIn = await page.evaluate(() => {
                const elements = document.querySelectorAll('.message-in .copyable-text');
                return Array.from(elements).map(el => el.innerText);
            });

            const messagesOut = await page.evaluate(() => {
                const elements = document.querySelectorAll('.message-out .copyable-text');
                return Array.from(elements).map(el => el.innerText);
            });

            const imagesIn = await page.evaluate(() => {
                const elements = document.querySelectorAll('.message-in img');
                return Array.from(elements).map(el => el.src);
            });

            const imagesOut = await page.evaluate(() => {
                const elements = document.querySelectorAll('.message-out img');
                return Array.from(elements).map(el => el.src);
            });

            const encodedImagesIn = await getEncodedImages(imagesIn);
            const encodedImagesOut = await getEncodedImages(imagesOut);

            requestBody.push({
                'ChatName': chatTitle,
                'MessagesIn': [...new Set(messagesIn)],
                'MessagesOut': [...new Set(messagesOut)],
                'ImagesIn': [],//encodedImagesIn,
                'ImagesOut': [],//encodedImagesOut
            });
        }

        console.log('requestBody', requestBody);

        const response = await fetch('https://profanitiesprotector.azurewebsites.net/ProfanitiesProtector/analyze', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json; charset=utf-8'
            },
            body: JSON.stringify({ "Content": requestBody, "Email": email })
        });
        console.log(JSON.stringify({ "Content": requestBody, "Email": email }));       
    
        await browser.close();
    
        res.json({ message: 'API call done. Check console for details.' });
    
    })();
    
    });
    
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
    
    app.get('/', (req, res) => {
        res.sendFile(path.join(__dirname, 'public', 'index.html'));
    });
    
    app.listen(port, () => {
        console.log(`Server is running at http://localhost:${port}`);
    });