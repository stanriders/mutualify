module.exports = {
  reactStrictMode: true,
  compiler: {
    emotion: true,
  },
  output: 'standalone',
  async headers() {
    return [
      {
        source: '/:path*',
        headers: [
          {
            key: 'Referrer-Policy',
            value: 'origin-when-cross-origin'
          }
        ],
      },
    ]
  },
};
