/**
 * ImageConversionのローカルテスト通信モジュール
 */

const request = require('request');
const TestData = require('./ImageConversionTestData');

const data = TestData.testData;

//ヘッダーを定義
const headers = {
    'Content-Type':'application/json',
};

//オプションを定義
const requestUrl = `${TestData.requestUrl}/api/ImageConversion`;

const options = {
    url: requestUrl,
    headers: headers,
    json: data,
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