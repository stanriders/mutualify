# https://github.com/vercel/next.js/blob/canary/examples/with-docker/Dockerfile
FROM node:16 AS deps

WORKDIR /app

COPY package*.json ./

RUN npm install

# Rebuild the source code only when needed
FROM node:16 AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
# have to pass this envvar into the build container to make it build proper paths for the client-facing js
ENV NEXT_PUBLIC_API_ADDRESS $NEXT_PUBLIC_API_ADDRESS

RUN npm run build

# Production image, copy all the files and run next
FROM node:16 AS runner
WORKDIR /app

ENV NODE_ENV production

COPY --from=builder /app/next.config.js ./
COPY --from=builder /app/public ./public
COPY --from=builder /app/package.json ./package.json

# Automatically leverage output traces to reduce image size 
# https://nextjs.org/docs/advanced-features/output-file-tracing
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static

EXPOSE 3000

ENV PORT 3000

# https://nextjs.org/telemetry
ENV NEXT_TELEMETRY_DISABLED 1

CMD ["node", "server.js"]