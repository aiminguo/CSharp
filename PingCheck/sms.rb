require 'rest_client'
require 'json'
#* 20 * * * webdev ruby /var/webdev/sms.rb
def lookup_server(key)
	serverList = {	"dev11" =>  "https://tpodev11-8i.corp.homestore.net/Notification",
			"itgn" => "https://sms-itgn.topproduceronline.com",
			"sqan" => "https://sms-sqan.topproduceronline.com", 
			"stag" => "https://sms-staging.topproduceronline.com",
			"prod" => "https://sms-service.realtor.com",
			"dev10" => "http://tpodev10web01.tpolab.com:38570",
			"local" => "http://localhost:38570"
	}
	serverList[key] 
end

def send_sms(url, request)
	begin
		puts url
		time_before = Time.now

		response = RestClient.post url,  request, :content_type => :json
		puts response.body,  Time.now - time_before
	rescue StandardError=>ex
		request = {'token' => 'E59F3284-DB48-4D71-89E8-010025CA45BB','sms' => {'fromAddress' => 'la@Realtor.com','fromName' => 'Realtor.com','subject' => 'SMS service error from ' + url, 'toAddress' => 'fake@fake.com', 'textBody' => ex}}.to_json
		RestClient.post "#{lookup_server('prod')}/Notification.svc/sendAlert",  request, :content_type => :json
	end
end

request = {'token' => 'E59F3284-DB48-4D71-89E8-010025CA45BB','sms' => {'client' => 'ruby test','fromAddress' => 'la@Realtor.com','fromName' => 'Realtor.com','subject' => 'LPS Lead SMS Message','toAddress' => 'tposmsmonitor@move.com'}}.to_json

lineups = ['prod', 'stag', 'sqan', 'itgn']
lineups.each do |server|
	url = "#{lookup_server(server)}/Notification.svc/sendAlert"
	send_sms url, request
end

