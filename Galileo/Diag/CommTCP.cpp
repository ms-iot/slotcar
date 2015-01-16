#include "CommTCP.h"
#include "arduino.h"

#include <memory>
#include <utility>
#include <sstream>
#include <thread>

// The binary helper causes build errors in boost
#undef B1
#undef boolean
#include <boost/property_tree/ptree.hpp>
#include <boost/property_tree/json_parser.hpp>
#include <boost/asio.hpp>

using boost::asio::ip::tcp;
using boost::property_tree::ptree;
using boost::property_tree::read_json;

class session
	: public std::enable_shared_from_this<session>
{
public:
	session(tcp::socket socket, uint16_t pwm)
		: _socket(std::move(socket))
		, _pwm(pwm)
	{
		Log("***%d Session Starting\n", _pwm);
	}

	void start()
	{
		do_read();
	}

private:
	void do_read()
	{
		ZeroMemory(_data, sizeof(_data));

		auto self(shared_from_this());
		_socket.async_read_some(boost::asio::buffer(_data, max_length),
			[this, self](boost::system::error_code ec, std::size_t length)
		{
			ptree pt;
			std::istringstream is(_data);
			read_json(is, pt);
			std::string foo = pt.get<std::string>("foo");

			analogWrite(_pwm, 0);
		});
	}

	uint16_t _pwm;
	tcp::socket _socket;
	enum { max_length = 1024 };
	char _data[max_length];
};

class server
{
public:
	server(boost::asio::io_service& io_service, short port, uint16_t pwm)
		: _acceptor(io_service, tcp::endpoint(tcp::v4(), port))
		, _socket(io_service)
		, _pwm(pwm)
	{
		do_accept();
	}

private:
	void do_accept()
	{
		_acceptor.async_accept(_socket,
			[this](boost::system::error_code ec)
		{
			if (!ec)
			{
				auto s = std::make_shared<session>(std::move(_socket), _pwm);

				s->start();
			}

			do_accept();
		});
	}

	tcp::acceptor _acceptor;
	tcp::socket _socket;
	uint16_t _pwm;
};

int dir1PinA = A5;
int dir2PinA = A4;
int speedPinA = 9; // Needs to be a PWM pin to be able to control motor speed

// Motor 2
int dir1PinB = A3;
int dir2PinB = A2;
int speedPinB = 10; // Needs to be a PWM pin to be able to control motor speed

const uint16_t Game1Port = 25666;
const uint16_t Game2Port = 25667;

boost::asio::io_service io_service;

server game1(io_service, Game1Port, speedPinA);
server game2(io_service, Game2Port, speedPinB);

std::thread io_serviceThread([&](){ io_service.run(); });


void CommTCP::Initialize()
{
	pinMode(dir1PinA, OUTPUT);
	pinMode(dir2PinA, OUTPUT);
	pinMode(speedPinA, OUTPUT);
	pinMode(dir1PinB, OUTPUT);
	pinMode(dir2PinB, OUTPUT);
	pinMode(speedPinB, OUTPUT);

	analogWrite(speedPinA, 0);
	digitalWrite(dir1PinA, LOW);
	digitalWrite(dir2PinA, HIGH);

	analogWrite(speedPinB, 0);
	digitalWrite(dir1PinB, LOW);
	digitalWrite(dir2PinB, HIGH);


}

//
//int _tmain(int argc, _TCHAR* argv[])
//{
//	return RunArduinoSketch();
//}
//
//Keyboard Controls:
//
// 1 -Motor 1 Left
// 2 -Motor 1 Stop
// 3 -Motor 1 Right
//
// 4 -Motor 2 Left
// 5 -Motor 2 Stop
// 6 -Motor 2 Right

//// Motor 1
//int dir1PinA = 2;
//int dir2PinA = 3;
//int speedPinA = 9; // Needs to be a PWM pin to be able to control motor speed
//
//// Motor 2
//int dir1PinB = 4;
//int dir2PinB = 5;
//int speedPinB = 10; // Needs to be a PWM pin to be able to control motor speed
//
//int ramp = 0;
//bool up = true;
//const uint16_t Game1Port = 25666;
//const uint16_t Game2Port = 25666;
//
//boost::asio::io_service io_service;
//std::thread io_serviceThread([](){ io_service.run(); });
//
//server game1(io_service, Game1Port, speedPinA);
//server game2(io_service, Game1Port, speedPinB);

//
//void setup()
//{
//	pinMode(dir1PinA, OUTPUT);
//	pinMode(dir2PinA, OUTPUT);
//	pinMode(speedPinA, OUTPUT);
//	pinMode(dir1PinB, OUTPUT);
//	pinMode(dir2PinB, OUTPUT);
//	pinMode(speedPinB, OUTPUT);
//
//	analogWrite(speedPinA, 0);
//	digitalWrite(dir1PinA, LOW);
//	digitalWrite(dir2PinA, HIGH);
//
//	analogWrite(speedPinB, 0);
//	digitalWrite(dir1PinB, LOW);
//	digitalWrite(dir2PinB, HIGH);
//}
//
//void loop()
//{
//}