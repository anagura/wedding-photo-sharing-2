/**
 * ImageConversionのローカルテストデータサンプル(LineReceiverTestData.jsをコピペ作成用)
 */

module.exports.requestUrl = 'http://localhost:7071';
module.exports.channelSecret = 'xxx';
module.exports.testData = {
    "events": [
        {
            "type": "message",
            "replyToken": "xxx",
            "source": {
                "userId": "xxx",
                "type": "user"
            },
            "timestamp": 123,
            "mode": "active",
            "message": {
                "type": "text",
                "type": "text",
                "id": "123",
                "text": "xxx"
            }
        }
    ],
    "destination": "xxx"
};