#!/usr/bin/env sh

LOCALSTACK_HOST=localhost
AWS_REGION=us-east-1
LOCALSTACK_DUMMY_ID=000000000000

aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name public-api.fifo --attributes FifoQueue=true
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name internal-api2.fifo --attributes FifoQueue=true