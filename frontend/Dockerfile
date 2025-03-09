# syntax=docker/dockerfile:1.7-labs 

# Frontend Dockerfile using Next.js

FROM node:23-alpine AS build
WORKDIR /app

# Install dependencies & packages
COPY package*.json ./
RUN npm install

# Copy the rest of the frontend code
COPY --exclude=**/.env* --exclude=.next --exclude=node_modules . .

# Build the application
RUN npm run build

# Production stage
FROM node:23-alpine AS production
WORKDIR /app


ENV NODE_ENV=production

RUN addgroup -g 1001 -S nodejs
RUN adduser -S nextjs -u 1001

# Copy built assets from build stage to minimize the size of the container
COPY --from=build /app/package*.json ./
COPY --from=build /app/next.config.mjs ./
COPY --from=build /app/public ./public
COPY --from=build --chown=nextjs:nodejs /app/.next ./.next

RUN rm -fr /app/.next/cache || true

# Install production dependencies
RUN npm install next

USER nextjs
COPY --from=build /app/start.sh .

ENV LISTEN=0.0.0.0
ENV HOST=localhost
ENV PORT=3000
ENV BACKEND_PORT=5000

# Expose the port the app runs on
EXPOSE ${PORT}

# ENV NEXT_PUBLIC_FRONTEND_URL="https://${HOST}:${PORT}"
# ENV NEXT_PUBLIC_BACKEND_URL="https://${HOST}:${BACKEND_PORT}"

# Start the application
# CMD ["npx", "next", "start"]
ENTRYPOINT ["sh"]
CMD ["start.sh"]