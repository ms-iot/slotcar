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

    virtual ~session()
    {
        Log("***%d Session ending\n", _pwm);
    }

private:
    
    void do_read()
	{
		ZeroMemory(_data, sizeof(_data));

		auto self(shared_from_this());
		_socket.async_read_some(boost::asio::buffer(_data, max_length),
			[this, self](boost::system::error_code ec, std::size_t length)
		{
            if ((boost::asio::error::eof == ec) ||
                (boost::asio::error::connection_reset == ec))
            {
                // handle the disconnect, motor to zero
                analogWrite(_pwm, 0);

                // Return from here prevents another read, 
                // which releases and cleans up this session
                return;
            }
            else
            {
                try
                {
                    // Expecting JSON blobs. Read only a single line
                    ptree pt;
                    std::string line;
                    std::istringstream is(_data);
                    std::getline(is, line);

                    std::stringstream liness(line);
                    read_json(liness, pt);
                    std::string spwm = pt.get<std::string>("PWM");

                    uint16_t pwm = atoi(spwm.c_str());

                    //Log("PWM: %d\n", pwm);
                    
                    if (pwm > 0 && pwm < 255)
                    {
                        analogWrite(_pwm, pwm);
                    }
                }
                catch (const std::exception& e)
                {
                    Log("Help! %s", e.what());
                }

                do_read();
            }		
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

void CommTCP::Initialize()
{
	pinMode(dir1PinA, OUTPUT);
	pinMode(dir2PinA, OUTPUT);
	pinMode(speedPinA, OUTPUT);
	pinMode(dir1PinB, OUTPUT);
	pinMode(dir2PinB, OUTPUT);
	pinMode(speedPinB, OUTPUT);

    analogWrite(speedPinA, 0);
    digitalWrite(dir1PinA, HIGH);
    digitalWrite(dir2PinA, LOW);

    analogWrite(speedPinB, 0);
    digitalWrite(dir1PinB, LOW);
    digitalWrite(dir2PinB, HIGH);


}

boost::asio::io_service io_service;

const uint16_t Game1Port = 25666;
const uint16_t Game2Port = 25667;

int speedPinA = D10; // Needs to be a PWM pin to be able to control motor speed
int speedPinB = D9; // Needs to be a PWM pin to be able to control motor speed

server game1(io_service, Game1Port, speedPinA);
server game2(io_service, Game2Port, speedPinB);

std::thread io_serviceThread([&](){ io_service.run(); });
