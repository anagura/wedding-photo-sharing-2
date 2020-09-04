/**
 * LineReceiverのローカルテスト通信モジュール
 */

const request = require('request');
const crypto = require('crypto');
const LineReceiverTestData = require('./LineReceiverTestData');

const password = LineReceiverTestData.channelSecret;
const data = LineReceiverTestData.testData;

// signatureの生成
const signature = crypto
.createHmac('sha256', password)
.update(Buffer.from(JSON.stringify(data)))
.digest('base64');

//ヘッダーを定義
const headers = {
    'Content-Type':'application/json',
    "X-Line-Signature": signature,
};

//オプションを定義
const requestUrl = 'http://localhost:7071/api/LineReceiver'
const formData = data;

const options = {
    url: requestUrl,
    headers: headers,
    json: formData,
};

// 通信
request.post(options, (error, response, body) => {
  if (error !== null) {
    console.error('error:', error);
    return(false);
  }

  console.log('statusCode:', response && response.statusCode);
  console.log('body:', body);
});