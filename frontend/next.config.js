module.exports = {
  reactStrictMode: true,
  compiler: {
    emotion: true,
  },
  i18n: {
    locales: ["en-US", "fi-FI", "fr-FR", "pl-PL", "ru-RU", "sv-SE", "zh-CN", "zh-TW"],
    defaultLocale: "en-US"
  },
  output: 'standalone'
};
