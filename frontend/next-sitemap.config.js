/** @type {import('next-sitemap').IConfig} */
module.exports = {
    siteUrl: process.env.SITE_URL || 'https://mutualify.stanr.info',
    generateRobotsTxt: true, // (optional)
    exclude: ['/500', '/*/500'],
    // ...other options
  }