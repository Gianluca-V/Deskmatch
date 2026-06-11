import puppeteer from 'puppeteer';

(async () => {
  try {
    const browser = await puppeteer.launch({
      headless: 'new',
      args: ['--no-sandbox', '--disable-setuid-sandbox']
    });
    
    const page = await browser.newPage();

    page.on('console', msg => console.log('PAGE LOG:', msg.text()));
    page.on('error', err => console.error('PAGE ERROR:', err));

    // Establecer localStorage antes de navegar
    await page.evaluateOnNewDocument(() => {
      const fakeSession = {
        accessToken: 'fake-token-for-testing',
        user: {
          id: 'test-user-1',
          firstName: 'Carlos',
          lastName: 'Ruiz',
          email: 'owner@example.com',
          role: 'manager'
        }
      };
      localStorage.setItem('dm_session', JSON.stringify(fakeSession));
    });
    
    await page.goto('http://localhost:5173/profile/company', { waitUntil: 'networkidle2', timeout: 30000 });
    await new Promise(r => setTimeout(r, 2000)); // Esperar a que se renderice
    await page.screenshot({ path: '/tmp/company-profile-complete.png', fullPage: true });
    
    await browser.close();
    console.log('Screenshot saved');
  } catch (e) {
    console.error('Error:', e.message);
    process.exit(1);
  }
})();
